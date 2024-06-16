using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyserSubscriber.Events.Decorators;

public class AnalyserLogDecorator : IAnalyserLogDecorator
{
    private readonly ILogger<AnalyserLogDecorator> _logger;

    public AnalyserLogDecorator(ILogger<AnalyserLogDecorator> logger)
    {
        _logger = logger;
    }


    public async Task Decorate(Func<Task> function, WorldScan[] parameter)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await function.Invoke();
            stopwatch.Stop();

            _logger.LogInformation(
                "WorldScan({worldScanId}) - World({worldId}). Method '{methodName}', execution time : {time} ms.",
                parameter[0].WorldScanId, parameter[0].WorldId, function.Method.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "WorldScan({worldScanId}) - World({WorldId}) - Execution {methodName} cause error. Exception {exception}",
                parameter[0].WorldScanId, parameter[0].WorldId, function.Method.Name, exception);
            throw;
        }
    }

    public async Task Decorate(Func<WorldScan[], Task> function, WorldScan[] parameter)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await function.Invoke(parameter);
            stopwatch.Stop();

            _logger.LogInformation(
                "WorldScan({worldScanId}) - World({worldId}). Method '{methodName}', execution time : {time} ms.",
                parameter[0].WorldScanId, parameter[0].WorldId, function.Method.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "WorldScan({worldScanId}) - World({WorldId}) - Execution {methodName} causes error. Exception {exception}",
                parameter[0].WorldScanId, parameter[0].WorldId, function.Method.Name, exception);
            throw;
        }
    }
}