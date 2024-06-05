using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyser.Decorators;

public class AnalyserLogDecorator : IAnalyserLogDecorator
{
    private readonly ILogger<AnalyserLogDecorator> _logger;

    public AnalyserLogDecorator(ILogger<AnalyserLogDecorator> logger)
    {
        _logger = logger;
    }


    public async Task Decorate(Func<Task> function, List<WorldScan> parameter)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await function.Invoke();
            stopwatch.Stop();

            _logger.LogInformation(
                "WorldScans({worldScanId1}/{worldScanId2}) - World({worldId}). Method '{methodName}', execution time : {time} ms.",
                parameter[0].WorldScanId, parameter[1].WorldScanId, parameter[0].WorldId, function.Method.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "WorldScans({worldScanId1}/{worldScanId2}) - World({WorldId}) - Execution {methodName} cause error. Exception {exception}",
                parameter[0].WorldScanId, parameter[1].WorldScanId, parameter[0].WorldId, function.Method.Name, exception);
            throw;
        }
    }

    public async Task Decorate(Func<List<WorldScan>, Task> function, List<WorldScan> parameter)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await function.Invoke(parameter);
            stopwatch.Stop();

            _logger.LogInformation(
                "WorldScans({worldScanId1}/{worldScanId2}) - World({worldId}). Method '{methodName}', execution time : {time} ms.",
                parameter[0].WorldScanId, parameter[1].WorldScanId, parameter[0].WorldId, function.Method.Name, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "WorldScans({worldScanId1}/{worldScanId2}) - World({WorldId}) - Execution {methodName} causes error. Exception {exception}",
                parameter[0].WorldScanId, parameter[1].WorldScanId, parameter[0].WorldId, function.Method.Name, exception);
            throw;
        }
    }
}