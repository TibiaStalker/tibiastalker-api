namespace WorldScanAnalyserSubscriber.Subscribers;

public interface IWorldScansAnalyserSubscriber
{
    void Subscribe();
    void CloseChannels();
}