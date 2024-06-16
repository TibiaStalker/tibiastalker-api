namespace Shared.RabbitMQ.Events;

/// <summary>
/// Event to analyse two world scans
/// </summary>
/// <param name="WorldScanId1"></param>
/// <param name="WorldScanId2"></param>
public record WorldScansAnalyserEvent(int WorldScanId1, int WorldScanId2) : IntegrationEvent;
