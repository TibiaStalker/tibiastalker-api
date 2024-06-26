﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Infrastructure.Clients.TibiaData;
using TibiaStalker.Infrastructure.Policies;
using TibiaStalker.Infrastructure.Traceability;

namespace TibiaStalker.Infrastructure.Configuration;

public static class ConfigureApplication
{
    public static IServiceCollection AddTibiaDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration,
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
            .AddPolicyHandler(CommunicationPolicies.GetTibiaDataRetryPolicy());

        return services;
    }
}