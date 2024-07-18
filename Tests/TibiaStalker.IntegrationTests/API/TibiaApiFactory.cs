using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Shared.RabbitMQ.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using TibiaStalker.Api;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Infrastructure.Clients.TibiaData;
using TibiaStalker.Infrastructure.Configuration;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.Infrastructure.Policies;
using TibiaStalker.IntegrationTests.API.DatabaseSeeders;
using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.API;

public class TibiaApiFactory : WebApplicationFactory<Startup>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public readonly MockHttpMessageHandler MockHttp;

    public TibiaApiFactory()
    {
        _postgresContainer = new PostgreSqlContainerTest().Container;
        _rabbitMqContainer = new RabbitMqContainerTest().Container;
        MockHttp = new MockHttpMessageHandler();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseTestServer(options => options.PreserveExecutionContext = true);

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TibiaStalkerDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddSingleton(Options.Create(new ConnectionStringsSection { PostgreSql = _postgresContainer.GetConnectionString() })); // To satisfy Dapper
            services.AddDbContext<TibiaStalkerDbContext>(options => options.UseNpgsql(_postgresContainer.GetConnectionString()).UseSnakeCaseNamingConvention());

            services.AddScoped<IInitializer, TestDatabaseApiInitializer>(); // To automate db migrations

            services.RemoveAll<ITibiaDataClient>();
            services.AddTransient<ITibiaDataClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<TibiaDataSection>>();
                var logger = provider.GetRequiredService<ILogger<TibiaDataV3Client>>();
                var retryPolicy = provider.GetRequiredService<ITibiaDataRetryPolicy>();
                return new TibiaDataV3Client(new HttpClient(MockHttp)
                {
                    BaseAddress = new Uri($"{options.Value.BaseAddress}")
                }, options, logger, retryPolicy);
            });
        });

        var rabbitPort = _rabbitMqContainer.GetMappedPublicPort(5672).ToString();
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>($"{RabbitMqSection.SectionName}:{nameof(RabbitMqSection.Port)}", rabbitPort)
            }!);
        });
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}