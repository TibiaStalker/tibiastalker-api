using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace ChangeNameDetector.Services;

public class ChangeNameDetectorService : IChangeNameDetectorService
{
    private readonly ILogger<ChangeNameDetectorService> _logger;
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly IEventPublisher _publisher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ChangeNameDetectorSection _changeNameDetectorOptions;

    public ChangeNameDetectorService(ILogger<ChangeNameDetectorService> logger,
        ITibiaStalkerDbContext dbContext,
        IEventPublisher publisher,
        IOptions<SeederVariablesSection> options,
        IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _dbContext = dbContext;
        _publisher = publisher;
        _dateTimeProvider = dateTimeProvider;
        _changeNameDetectorOptions = options.Value.ChangeNameDetector;
    }

    public async Task<bool> Run()
    {
            var stopwatch = Stopwatch.StartNew();

            var character = await GetFirstCharacterByVerifiedDateAsync();

            if (character is null)
            {
                return false;
            }

            _logger.LogInformation("Get character '{characterName}' from DB. Execution time : {time} ms",
                character.Name, stopwatch.ElapsedMilliseconds);

            character.VerifiedDate = _dateTimeProvider.DateOnlyUtcNow;
            await _dbContext.SaveChangesAsync();

            await _publisher.PublishAsync($"'{character}' ({_dateTimeProvider.DateTimeUtcNow})", new ChangeNameDetectorEvent(character.Name));
            _logger.LogInformation("Character '{characterName}' checked. Execution time : {time} ms", character.Name, stopwatch.ElapsedMilliseconds);

            return true;
    }

    private async Task<Character> GetFirstCharacterByVerifiedDateAsync()
    {
        // Sign "-" is for back time
        var visibilityOfTradePeriodDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-_changeNameDetectorOptions.VisibilityOfTradePeriod);
        var scanPeriodDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-_changeNameDetectorOptions.ScanPeriod);

        return await _dbContext.Characters
            .Where(c => c.TradedDate < visibilityOfTradePeriodDate && c.VerifiedDate < scanPeriodDate)
            .OrderBy(c => c.VerifiedDate)
            .FirstOrDefaultAsync();
    }
}