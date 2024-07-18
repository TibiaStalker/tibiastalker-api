using Microsoft.Extensions.DependencyInjection;
using WorldSeeder.Decorators;

namespace WorldSeeder.Configuration;

public static class WorldSeederServicesConfiguration
{
    public static IServiceCollection AddWorldSeeder(this IServiceCollection services)
    {
        services.AddScoped<IWorldSeederService, WorldSeederService>();
        services.AddScoped<WorldService>();
        services.AddScoped<IWorldSeederLogDecorator, WorldSeederLogDecorator>();

        return services;
    }
}