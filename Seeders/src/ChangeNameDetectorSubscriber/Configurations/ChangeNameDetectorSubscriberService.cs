using ChangeNameDetectorSubscriber.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeNameDetectorSubscriber.Configurations;

public static class ChangeNameDetectorSubscriberService
{
    public static IServiceCollection AddChangeNameDetectorSubscriberService(this IServiceCollection services)
    {
        services.AddSingleton<Subscribers.ChangeNameDetectorRabbitSubscriber>();
        services.AddSingleton<IEventResultHandler, EventResultHandler>();

        return services;
    }
}