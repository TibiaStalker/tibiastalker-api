namespace WorldScanAnalyserSubscriber.Handlers;

public interface IEventResultHandler
{
    void HandleTransactionResult(bool isCommitedProperly, string eventName, string payload, int worldScanId1, int worldScanId2);
}