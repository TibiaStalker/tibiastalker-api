using System.Transactions;
using Microsoft.Extensions.Logging;

namespace WorldScanAnalyserSubscriber.Handlers;

public class EventResultHandler : IEventResultHandler
{
    private readonly ILogger<EventResultHandler> _logger;

    public EventResultHandler(ILogger<EventResultHandler> logger)
    {
        _logger = logger;
    }
    public void HandleTransactionResult(bool isCommitedProperly, string eventName, string payload, int worldScanId1, int worldScanId2)
    {
        switch (isCommitedProperly)
        {
            case true:
                _logger.LogInformation("Transaction '{event}' commited properly. World Scans Ids '{worldScanId1}/{worldScanId2}'. Payload {payload}",
                    eventName, worldScanId1, worldScanId2, payload);
                break;
            case false:
                _logger.LogError("Transaction '{event}' failed. Check dead letter for analyse problem. Payload {payload}",
                    eventName, payload);
                throw new TransactionException($"Transaction commited improperly during event '{eventName}'. Payload: {payload}.");
        }
    }
}