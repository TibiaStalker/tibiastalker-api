using Microsoft.Extensions.DependencyInjection;
using WorldScanSeeder.Decorators;

namespace WorldScanSeeder.Services;

public static class WorldScanSeederService
{
    public static IServiceCollection AddWorldScanSeederServices(this IServiceCollection services)
    {
        services.AddScoped<IScanSeeder, ScanSeeder>();
        services.AddScoped<WorldScanService>();
        services.AddScoped<IScanSeederLogDecorator, ScanSeederLogDecorator>();

        return services;
    }
}