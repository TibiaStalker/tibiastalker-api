using System.Reflection;
using ChangeNameDetectorSubscriber.Configurations;
using ChangeNameDetectorSubscriber.Subscribers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared.RabbitMQ.Extensions;
using Shared.RabbitMQ.Initializers;
using TibiaStalker.Infrastructure.Builders;

namespace ChangeNameDetectorSubscriber;

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
                    services.AddChangeNameDetectorSubscriberService();
                    services.AddRabbitMqSubscriber(context.Configuration, projectName);
                },
                builder => builder.RegisterEventSubscribers());

            Log.Information("Starting application");

            var initializer = ActivatorUtilities.CreateInstance<InitializationRabbitMqTaskRunner>(host.Services);
            await initializer.StartAsync();
            var service = ActivatorUtilities.CreateInstance<ChangeNameDetectorRabbitSubscriber>(host.Services);
            service.Subscribe();
            await host.WaitForShutdownAsync();

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