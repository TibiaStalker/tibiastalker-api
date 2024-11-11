using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.RabbitMQ.Configuration;
using Shared.RabbitMQ.Extensions;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Infrastructure.Clients.TibiaData;
using TibiaStalker.Infrastructure.HealthChecks;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.Infrastructure.Policies;
using TibiaStalker.Infrastructure.Services.BackgroundServices;
using TibiaStalker.Infrastructure.Traceability;

namespace TibiaStalker.Infrastructure.Configuration;

public static class ConfigureApplication
{
    public static IServiceCollection AddTibiaDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TibiaStalkerDbContext>(opt => opt
            .UseNpgsql(
                configuration.GetConnectionString(nameof(ConnectionStringsSection.PostgreSql)),
                options =>
                {
                    options
                        .MinBatchSize(1)
                        .MaxBatchSize(30)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                        .CommandTimeout(configuration
                            .GetSection(
                                $"{EfCoreConfigurationSection.SectionName}:{EfCoreConfigurationSection.CommandTimeout}")
                            .Get<int>());
                })
            .UseSnakeCaseNamingConvention());

        return services;
    }

    public static IServiceCollection AddExternalSerilog(this IServiceCollection services, IConfiguration configuration,
        string projectName)
    {
        LoggerConfiguration.ConfigureLogger(configuration, projectName);
        return services;
    }

    public static IServiceCollection AddTibiaHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ITibiaDataClient, TibiaDataV3Client>("TibiaDataClient", httpClient =>
            {
                httpClient.BaseAddress = new Uri(configuration[$"{TibiaDataSection.SectionName}:{nameof(TibiaDataSection.BaseAddress)}"]!);
                httpClient.Timeout = TimeSpan.Parse(configuration[$"{TibiaDataSection.SectionName}:{nameof(TibiaDataSection.Timeout)}"]!);
            })
            .AddHttpMessageHandler<HttpClientDecompressionHandler>()
            .AddPolicyHandler(CommunicationPolicies.GetHttpClientRetryPolicy());

        return services;
    }

    public static IServiceCollection AddTibiaHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetSection(RabbitMqSection.SectionName);
        var rabbitOptions = rabbitSection.Get<RabbitMqSection>();

        services.AddHealthChecks()
            .AddRabbitMQ(op => op.ConnectionFactory = rabbitOptions.GetRabbitMqConnectionFactory())
            .AddCheck<DatabaseHealthCheck>("Database");

        return services;
    }

    public static IServiceCollection AddTibiaOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<TibiaDataSection>()
            .Bind(configuration.GetSection(TibiaDataSection.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SeederVariablesSection>()
            .Bind(configuration.GetSection(SeederVariablesSection.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ConnectionStringsSection>()
            .BindConfiguration(ConnectionStringsSection.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DapperConfigurationSection>()
            .BindConfiguration(DapperConfigurationSection.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<BackgroundServiceTimerSection>()
            .BindConfiguration(BackgroundServiceTimerSection.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}