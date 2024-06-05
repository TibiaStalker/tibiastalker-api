using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.RabbitMQ.Extensions;
using Shared.RabbitMQ.Initializers;
using TibiaStalker.Infrastructure.Builders;
using WorldScanAnalyser.Configuration;

namespace WorldScanAnalyser;

public class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var projectName = Assembly.GetExecutingAssembly().GetName().Name;

            var host = CustomHostBuilder.Create(
                projectName,
                (context, services) =>
                {
                    services.AddCharacterAnalyser();
                    services.AddRabbitMqPublisher(context.Configuration, projectName);
                });

            Log.Information("Starting application");

            var initializer = ActivatorUtilities.CreateInstance<InitializationRabbitMqTaskRunner>(host.Services);
            await initializer.StartAsync();
            var service = ActivatorUtilities.CreateInstance<AnalyserService>(host.Services);
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
