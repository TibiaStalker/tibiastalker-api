using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RichardSzalay.MockHttp;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;
using WorldScanSeeder;

namespace TibiaStalker.IntegrationTests.Seeders.WorldScanSeeders;

[Collection(TestCollections.SeederCollection)]
public class WorldScanSeederTests : IAsyncLifetime
{
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTibiaDataResponses", "WorldsController", "GetWorldResponse.json");
    private const string TibiaDataWorldsEndpoint = "/v3/world/*";

    private readonly TibiaSeederFactory _factory;

    public WorldScanSeederTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WorldSeeder_Seed_ShouldCreateOnlyNewWorlds()
    {
        // Arrange
        var getWorldResponse = await File.ReadAllTextAsync(_filePath);

        _factory.MockHttp.When($"{TibiaDataWorldsEndpoint}")
            .Respond("application/json", getWorldResponse);

        using var scope = _factory.Services.CreateScope();
        var scanSeeder = scope.ServiceProvider.GetRequiredService<IScanSeeder>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await scanSeeder.SetProperties();
        await scanSeeder.Seed(scanSeeder.AvailableWorlds[0]);
        var worldScans = dbContext.WorldScans.AsNoTracking().ToList();

        // Assert
        worldScans.Count.Should().Be(1);
        worldScans[0].WorldId.Should().Be(11);
        worldScans[0].CharactersOnline.Should().Be("name-a|name-b|name-c|name-d|name-e|name-f");
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync<TestDatabaseSeederWorldSeeder>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}