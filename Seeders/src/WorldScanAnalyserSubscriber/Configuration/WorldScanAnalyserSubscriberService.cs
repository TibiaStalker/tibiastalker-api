using Microsoft.Extensions.DependencyInjection;
using WorldScanAnalyserSubscriber.Events;
using WorldScanAnalyserSubscriber.Events.Decorators;
using WorldScanAnalyserSubscriber.Handlers;
using WorldScanAnalyserSubscriber.Subscribers;

namespace WorldScanAnalyserSubscriber.Configuration;

public static class WorldScanAnalyserSubscriberService
{
    public static IServiceCollection AddWorldScanAnalyserSubscriberServices(this IServiceCollection services)
    {
        services.AddSingleton<WorldScansAnalyserSubscriber>();
        services.AddSingleton<IEventResultHandler, EventResultHandler>();
        services.AddScoped<IAnalyser, Analyser>();
        services.AddScoped<IAnalyserLogDecorator, AnalyserLogDecorator>();

        return services;
    }
}