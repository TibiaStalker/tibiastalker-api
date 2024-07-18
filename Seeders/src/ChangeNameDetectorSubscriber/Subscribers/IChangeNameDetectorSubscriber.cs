namespace ChangeNameDetectorSubscriber.Subscribers;

public interface IChangeNameDetectorSubscriber
{
    void Subscribe();
    void CloseChannels();
}