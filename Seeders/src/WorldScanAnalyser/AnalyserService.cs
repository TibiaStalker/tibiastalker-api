using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;

namespace WorldScanAnalyser;

public class AnalyserService : IAnalyserService
{
    private readonly ILogger<AnalyserService> _logger;
    private readonly IAnalyser _analyser;
    private readonly IEventPublisher _publisher;

    public AnalyserService(ILogger<AnalyserService> logger, IAnalyser analyser, IEventPublisher publisher)
    {
        _logger = logger;
        _analyser = analyser;
        _publisher = publisher;
    }

    public async Task Run()
    {
        while (true)
        {
            var worldIds = _analyser.GetDistinctWorldIdsFromRemainingScans();
            if (worldIds.Count == 0)
            {
                return;
            }

            foreach (var worldId in worldIds)
            {
                try
                {
                    var stopwatch = Stopwatch.StartNew();

                    var worldScans = _analyser.GetWorldScansToAnalyse(worldId);
                    _logger.LogInformation("GetWorldScansToAnalyseAsync - execution time: {time} ms.",
                        stopwatch.ElapsedMilliseconds);

                    await _publisher.PublishAsync($"'{worldScans[0].WorldScanId}/{worldScans[1].WorldScanId}' ({DateTime.Now})",
                        new WorldScansAnalyserEvent(worldScans[0].WorldScanId, worldScans[1].WorldScanId));

                    await _analyser.SoftDeleteWorldScanAsync(worldScans[0].WorldScanId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Execution of {methodName} failed", nameof(AnalyserService));
                }
            }
        }
    }
}