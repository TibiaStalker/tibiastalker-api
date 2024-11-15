﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;
using TibiaStalker.Application.Interfaces;

namespace WorldScanAnalyser;

public class AnalyserService : IAnalyserService
{
    private readonly ILogger<AnalyserService> _logger;
    private readonly IAnalyser _analyser;
    private readonly IEventPublisher _publisher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AnalyserService(ILogger<AnalyserService> logger, IAnalyser analyser, IEventPublisher publisher, IDateTimeProvider dateTimeProvider)
    {
        _logger = logger;
        _analyser = analyser;
        _publisher = publisher;
        _dateTimeProvider = dateTimeProvider;
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

                    var worldScans = _analyser.GetTwoWorldScansToAnalyse(worldId);
                    _logger.LogInformation("GetWorldScansToAnalyseAsync - execution time: {time} ms.",
                        stopwatch.ElapsedMilliseconds);

                    await _publisher.PublishAsync($"'{worldScans[0].WorldScanId}/{worldScans[1].WorldScanId}' ({_dateTimeProvider.DateTimeUtcNow})",
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