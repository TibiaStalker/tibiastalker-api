using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Serilog;
using Shared.RabbitMQ.Configuration;
using Shared.RabbitMQ.Conventions;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;
using Shared.RabbitMQ.Initializers;

namespace Shared.RabbitMQ.Extensions;

public static class RabbitMqExtension
{
    public static void AddRabbitMqPublisher(this IServiceCollection services, IConfiguration configuration, string connectionName)
    {
        services.AddRabbitMqCommonSettings(configuration, connectionName)
            .AddSingleton<IEventPublisher, RabbitMqPublisher>();
    }

    public static void AddRabbitMqSubscriber(this IServiceCollection services, IConfiguration configuration, string connectionName)
    {
        services.AddRabbitMqCommonSettings(configuration, connectionName);
    }

    public static ConnectionFactory GetRabbitMqConnectionFactory(this RabbitMqSection? options)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(options!.HostUrl),
            Port = options.Port,
            VirtualHost = options.VirtualHost,
            UserName = options.Username,
            Password = options.Password,
            DispatchConsumersAsync = true
        };

        return factory;
    }

    private static IServiceCollection AddRabbitMqCommonSettings(this IServiceCollection services, IConfiguration configuration, string connectionName)
    {
        var rabbitSection = configuration.GetSection(RabbitMqSection.SectionName);
        services.AddOptions<RabbitMqSection>()
            .Bind(rabbitSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var options = rabbitSection.Get<RabbitMqSection>();

        try
        {
            IConnection rabbitMqConnection = options.GetRabbitMqConnectionFactory().CreateConnection(connectionName);

            services
                .AddEvents()
                .AddSingleton<IRabbitMqConventionProvider, RabbitMqConventionProvider>()
                .AddSingleton(new RabbitMqConnection(rabbitMqConnection))
                .AddTransient<IRabbitMqInitializer, RabbitMqInitializer>()
                .AddSingleton<MessageSerializer>()
                .AddSingleton<InitializationRabbitMqTaskRunner>();
        }
        catch (BrokerUnreachableException ex)
        {
            Log.Warning("RabbitMq connection is closed. Error message: {Message}", ex.Message);
            Log.Warning("RabbitMq configuration: {RabbitConfig}", JsonSerializer.Serialize(options));
            throw;
        }

        return services;
    }

    private static IServiceCollection AddEvents(this IServiceCollection service)
    {
        service.AddSingleton(s =>
            new EventBusSubscriberBuilder(s.GetRequiredService<IRabbitMqConventionProvider>())
                .SubscribeEvent<WorldScansAnalyserEvent>().AsSelf()
                .SubscribeEvent<ChangeNameDetectorEvent>().AsSelf());

        return service;
    }
}