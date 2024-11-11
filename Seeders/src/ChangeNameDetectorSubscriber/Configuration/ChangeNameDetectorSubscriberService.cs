using ChangeNameDetectorSubscriber.Handlers;
using ChangeNameDetectorSubscriber.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeNameDetectorSubscriber.Configuration;

public static class ChangeNameDetectorSubscriberService
{
    public static IServiceCollection AddChangeNameDetectorSubscriberService(this IServiceCollection services)
    {
        services.AddSingleton<Subscribers.ChangeNameDetectorSubscriber>();
        services.AddSingleton<IEventResultHandler, EventResultHandler>();
        services.AddScoped<INameDetectorValidator, NameDetectorValidator>();

        return services;
    }
}