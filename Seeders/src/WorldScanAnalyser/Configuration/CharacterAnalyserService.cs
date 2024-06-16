using Microsoft.Extensions.DependencyInjection;
using WorldScanAnalyser.Decorators;

namespace WorldScanAnalyser.Configuration;

public static class CharacterAnalyserService
{
    public static IServiceCollection AddCharacterAnalyser(this IServiceCollection services)
    {
        services.AddScoped<IAnalyserService, AnalyserService>();
        services.AddScoped<IAnalyser, Analyser>();
        services.AddScoped<IAnalyserLogDecorator, AnalyserLogDecorator>();

        return services;
    }
}