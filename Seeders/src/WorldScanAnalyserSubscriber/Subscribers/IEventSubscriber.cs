using RabbitMQ.Client.Events;

namespace WorldScanAnalyserSubscriber.Subscribers;

public interface IEventSubscriber
{
    string GetQueueName();
    Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default);
}