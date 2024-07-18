using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RichardSzalay.MockHttp;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;
using WorldSeeder;

namespace TibiaStalker.IntegrationTests.Seeders.WorldSeeders;

[Collection(TestCollections.SeederCollection)]
public class WorldSeederTests : IAsyncLifetime
{
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestTibiaDataResponses", "WorldsController", "GetWorldsResponse.json");
    private const string TibiaDataWorldsEndpoint = "/v3/worlds";

    private readonly TibiaSeederFactory _factory;

    public WorldSeederTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WorldSeeder_Seed_ShouldCreateOnlyNewWorlds()
    {
        // Arrange
        var getWorldsResponse = await File.ReadAllTextAsync(_filePath);

        _factory.MockHttp.When($"{TibiaDataWorldsEndpoint}")
            .Respond("application/json", getWorldsResponse);

        using var scope = _factory.Services.CreateScope();
        var worldSeeder = scope.ServiceProvider.GetRequiredService<IWorldSeederService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await worldSeeder.SetProperties();
        await worldSeeder.Seed();
        var worlds = dbContext.Worlds.AsNoTracking().ToList();

        // Assert
        worlds.Count.Should().Be(6);
        worlds.Count(w => w.IsAvailable).Should().Be(5);
    }

    [Fact]
    public async Task WorldSeeder_TurnOffIfWorldIsUnavailable_ShouldSetWorldIsAvailableToFalse()
    {
        // Arrange
        var getWorldsResponse = await File.ReadAllTextAsync(_filePath);

        _factory.MockHttp.When($"{TibiaDataWorldsEndpoint}")
            .Respond("application/json", getWorldsResponse);

        using var scope = _factory.Services.CreateScope();
        var worldSeeder = scope.ServiceProvider.GetRequiredService<IWorldSeederService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await worldSeeder.SetProperties();
        await worldSeeder.TurnOffIfWorldIsUnavailable();
        var worlds = dbContext.Worlds.AsNoTracking().ToList();

        // Assert
        worlds.Count.Should().Be(5);
        worlds.Count(w => !w.IsAvailable).Should().Be(2);
    }

    [Fact]
    public async Task WorldSeeder_TurnOnIfWorldIsAvailable_ShouldSetWorldIsAvailableToTrue()
    {
        // Arrange
        var getWorldsResponse = await File.ReadAllTextAsync(_filePath);

        _factory.MockHttp.When($"{TibiaDataWorldsEndpoint}")
            .Respond("application/json", getWorldsResponse);

        using var scope = _factory.Services.CreateScope();
        var worldSeeder = scope.ServiceProvider.GetRequiredService<IWorldSeederService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await worldSeeder.SetProperties();
        await worldSeeder.TurnOnIfWorldIsAvailable();
        var worlds = dbContext.Worlds.AsNoTracking().ToList();

        // Assert
        worlds.Count.Should().Be(5);
        worlds.Count(w => w.IsAvailable).Should().Be(5);
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