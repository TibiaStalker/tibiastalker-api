using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using RichardSzalay.MockHttp;
using Shared.RabbitMQ.Configuration;
using Shared.RabbitMQ.Extensions;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using TibiaStalker.Api;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Infrastructure.Clients.TibiaData;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.Infrastructure.Policies;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

namespace TibiaStalker.IntegrationTests.Seeders;

public class TibiaSeederFactory : WebApplicationFactory<Startup>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;
    private readonly ITibiaDataRetryPolicy _tibiaDataRetryPolicy = Substitute.For<ITibiaDataRetryPolicy>();

    public MockHttpMessageHandler MockHttp;


    public TibiaSeederFactory()
    {
        _postgresContainer = new PostgreSqlContainerTest().Container;
        _rabbitMqContainer = new RabbitMqContainerTest().Container;
        MockHttp = new MockHttpMessageHandler();
        _tibiaDataRetryPolicy.GetRetryPolicy(Arg.Any<int>()).Returns(Policy.Handle<Exception>().WaitAndRetryAsync(TibiaDataV3Client.TotalRetryAttempts, a => TimeSpan.FromTicks(a)));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var rabbitPort = _rabbitMqContainer.GetMappedPublicPort(5672).ToString();
        IConfigurationRoot configuration = null;

        builder.UseTestServer(options => options.PreserveExecutionContext = true);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>($"{RabbitMqSection.SectionName}:{nameof(RabbitMqSection.Port)}", rabbitPort)
            }!);

            configuration = config.Build();
        });
        builder.ConfigureTestServices(services =>
        {
            services.AddProjectServices(TestAssemblies.ApplicationsAssembly);

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TibiaStalkerDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddSingleton(Options.Create(new ConnectionStringsSection { PostgreSql = _postgresContainer.GetConnectionString() })); // To satisfy Dapper
            services.AddDbContext<TibiaStalkerDbContext>(options => options.UseNpgsql(_postgresContainer.GetConnectionString()).UseSnakeCaseNamingConvention());

            services.AddRabbitMqPublisher(configuration!, "RabbitMqTest");


            services.RemoveAll<ITibiaDataClient>();
            services.AddTransient<ITibiaDataClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<TibiaDataSection>>();
                var logger = provider.GetRequiredService<ILogger<TibiaDataV3Client>>();
                return new TibiaDataV3Client(new HttpClient(MockHttp)
                {
                    BaseAddress = new Uri($"{options.Value.BaseAddress}")
                }, options, logger, _tibiaDataRetryPolicy);
            });
        });
    }

    public async Task ResetDatabaseAsync<T>() where T : ITestDatabaseSeeder
    {
        using var scope = Services.CreateScope();
        var testDatabaseSeeder = scope.ServiceProvider.GetRequiredService<T>();

        await testDatabaseSeeder.ResetDatabaseAsync();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        await _rabbitMqContainer.DisposeAsync();
    }
}