using RabbitMQ.Client.Events;

namespace ChangeNameDetectorSubscriber.Subscribers;

public interface IEventSubscriber
{
    string GetQueueName();
    Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default);
}