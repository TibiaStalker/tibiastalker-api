using Microsoft.Extensions.DependencyInjection;
using RabbitMqSubscriber.Handlers;
using WorldScanAnalyserSubscriber.Handlers;
using WorldScanAnalyserSubscriber.Subscribers;

namespace WorldScanAnalyserSubscriber.Configurations;

public static class RabbitMqSubscriberService
{
    public static IServiceCollection AddRabbitMqSubscriberServices(this IServiceCollection services)
    {
        services.AddSingleton<TibiaSubscriber>();
        services.AddSingleton<IEventResultHandler, EventResultHandler>();

        return services;
    }
}