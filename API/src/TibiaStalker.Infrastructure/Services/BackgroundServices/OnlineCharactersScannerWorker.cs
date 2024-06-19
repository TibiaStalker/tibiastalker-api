using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Infrastructure.Services.BackgroundServices;

public class OnlineCharactersScannerWorker : BackgroundService
{
    private readonly TimeSpan _period;

    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly ITibiaDataClient _tibiaDataClient;
    private readonly ILogger<OnlineCharactersScannerWorker> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public OnlineCharactersScannerWorker(ITibiaStalkerDbContext dbContext,
        ITibiaDataClient tibiaDataClient,
        ILogger<OnlineCharactersScannerWorker> logger,
        IOptions<BackgroundServiceTimerSection> options)
    {
        _dbContext = dbContext;
        _tibiaDataClient = tibiaDataClient;
        _logger = logger;
        _period = TimeSpan.FromSeconds(options.Value.OnlineCharactersScannerWorker);

        _retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_period);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (!await _dbContext.Database.CanConnectAsync(stoppingToken)) continue;

            var uniqueWorldNames = _dbContext.TrackedCharacters.Select(tc => tc.WorldName).Distinct().ToList();
            if (uniqueWorldNames.Count == 0) continue;

            var onlineCharacters = new List<OnlineCharacter>();

            foreach (var worldName in uniqueWorldNames)
            {
                var charactersNames = await _tibiaDataClient.FetchCharactersOnline(worldName);
                onlineCharacters.AddRange(charactersNames.Select(name =>
                    new OnlineCharacter(name.ToLower(), worldName)));
            }

            var currentRetry = 0;

            await _retryPolicy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(stoppingToken);

                try
                {
                    await _dbContext.OnlineCharacters.ExecuteDeleteAsync(cancellationToken: stoppingToken);
                    await _dbContext.OnlineCharacters.AddRangeAsync(onlineCharacters, stoppingToken);
                    await _dbContext.SaveChangesAsync(stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    currentRetry++;
                    await transaction.RollbackAsync(stoppingToken);
                    _logger.LogError(
                        "Method {method} during {action} failed, attempt {retryCount}. Error message: {ErrorMessage}",
                        nameof(ExecuteAsync), nameof(OnlineCharactersScannerWorker), currentRetry, ex.Message);
                }
            });
        }
    }
}