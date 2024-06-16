using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Shared.RabbitMQ.Conventions;
using Shared.RabbitMQ.Events;
using TibiaStalker.Infrastructure.Persistence;
using WorldScanAnalyserSubscriber.Handlers;
using WorldScanAnalyserSubscriber.Subscribers;

namespace WorldScanAnalyserSubscriber.Events;

public class WorldScansAnalyseEventSubscriber : IEventSubscriber
{
    private readonly ILogger<WorldScansAnalyseEventSubscriber> _logger;
    private readonly IEventResultHandler _eventResultHandler;
    private readonly IRabbitMqConventionProvider _conventionProvider;
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly IAnalyser _analyser;

    public WorldScansAnalyseEventSubscriber(
        ILogger<WorldScansAnalyseEventSubscriber> logger,
        IEventResultHandler eventResultHandler,
        IRabbitMqConventionProvider conventionProvider,
        ITibiaStalkerDbContext dbContext,
        IAnalyser analyser)
    {
        _logger = logger;
        _eventResultHandler = eventResultHandler;
        _conventionProvider = conventionProvider;
        _dbContext = dbContext;
        _analyser = analyser;
    }

    public string GetQueueName()
    {
        var queueBinding = _conventionProvider.GetForType<WorldScansAnalyserEvent>();
        return queueBinding.Queue;
    }

    public async Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        var payload = Encoding.UTF8.GetString(ea.Body.Span);
        var eventObject = JsonConvert.DeserializeObject<WorldScansAnalyserEvent>(payload);
        _logger.LogInformation("Event {Event} subscribed. Payload: {Payload}", eventObject.GetType().Name, payload);

        var worldScan1 = await _dbContext.WorldScans
            .Where(c => c.WorldScanId == eventObject.WorldScanId1)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var worldScan2 = await _dbContext.WorldScans
            .Where(c => c.WorldScanId == eventObject.WorldScanId2)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (worldScan1 is null || worldScan2 is null)
        {
            _logger.LogInformation(
                "During event {Event} - cannot find one of the world scans ({WorldScanId1}/{WorldScanId2}) in database. Payload: {Payload}",
                eventObject.GetType().Name, eventObject.WorldScanId1, eventObject.WorldScanId2, payload);
            return;
        }

        var isCommitedProperly = await ExecuteInTransactionAsync(Action, payload);

        _eventResultHandler.HandleTransactionResult(isCommitedProperly, nameof(WorldScansAnalyserEvent), payload, worldScan1.WorldScanId, worldScan2.WorldScanId);

        return;

        async Task Action()
        {
            await _analyser.Seed(new[] { worldScan1, worldScan2 });
        }
    }

    private async Task<bool> ExecuteInTransactionAsync(Func<Task> action, string payload)
    {
        for (int retryCount = 1; retryCount <= 3; retryCount++)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                await action.Invoke();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Method {method} during {action} failed, attempt {retryCount}. Payload {payload}. Error message: {ErrorMessage}",
                    nameof(ExecuteInTransactionAsync), action.Target?.GetType().ReflectedType?.Name, retryCount, payload, ex.Message);
            }
        }

        return false;
    }
}