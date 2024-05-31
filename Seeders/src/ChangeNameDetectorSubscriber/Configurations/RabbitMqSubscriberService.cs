using ChangeNameDetectorSubscriber.Handlers;
using ChangeNameDetectorSubscriber.Subscribers;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeNameDetectorSubscriber.Configurations;

public static class RabbitMqSubscriberService
{
    public static IServiceCollection AddRabbitMqSubscriberServices(this IServiceCollection services)
    {
        services.AddSingleton<TibiaSubscriber>();
        services.AddSingleton<IEventResultHandler, EventResultHandler>();

        return services;
    }
}