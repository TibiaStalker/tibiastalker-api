﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TibiaStalker.Infrastructure.Builders;
using WorldScanSeeder.Services;

namespace WorldScanSeeder;

public class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var projectName = Assembly.GetExecutingAssembly().GetName().Name;

            var host = CustomHostBuilder.Create(
                projectName,
                (_, services) => { services.AddWorldScanSeederServices(); });

            Log.Information("Starting application");

            var service = ActivatorUtilities.CreateInstance<WorldScanService>(host.Services);
            await service.Run();

            Log.Information("Ending application properly");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
