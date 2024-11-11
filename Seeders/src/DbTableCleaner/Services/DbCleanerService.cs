using DbCleaner.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace DbCleaner.Services;

public static class DbCleanerService
{
    public static IServiceCollection AddDbCleaner(this IServiceCollection services)
    {
        services.AddScoped<ICleaner, Cleaner>();
        services.AddScoped<CleanerService>();
        services.AddScoped<IDbCleanerLogDecorator, DbCleanerLogDecorator>();

        return services;
    }
}