using FluentAssertions;
using Shared.RabbitMQ.Initializers;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;
using WorldScanAnalyser;
using WorldScanAnalyserSubscriber.Subscribers;

namespace TibiaStalker.IntegrationTests.Seeders.WorldScanAnalysersSubscriber;

[Collection(TestCollections.RabbitCollection)]
public class WorldScansAnalyserSubscriberTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;

    private const int DelayMilliseconds = 500;

    public WorldScansAnalyserSubscriberTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WorldScansAnalyserSubscriber_Subscribe_ShouldReturnCorrectData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var rabbitMqInitializer = scope.ServiceProvider.GetRequiredService<InitializationRabbitMqTaskRunner>();
        var analyser = scope.ServiceProvider.GetRequiredService<IAnalyserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IWorldScansAnalyserSubscriber>();

        await rabbitMqInitializer.StartAsync();
        await analyser.Run();


        // Act
        await Subscribe(subscriber);


        // Assert
        var worldScans = dbContext.WorldScans.ToList();
        var characters = dbContext.Characters.ToList();
        var correlations = dbContext.CharacterCorrelations.ToList();

        worldScans.Count.Should().Be(4);
        worldScans.Where(ws => ws.IsDeleted).ToList().Count.Should().Be(2);
        worldScans.Where(ws => !ws.IsDeleted).ToList().Count.Should().Be(2);
        var correctWorldScanIdsInDatabase = new int[] { 114, 115, 224, 225 };
        worldScans.All(ws => correctWorldScanIdsInDatabase.Contains(ws.WorldScanId)).Should().BeTrue();

        characters.Count.Should().Be(6);
        var correctCharactersInDatabase = new string[] { "name-d", "name-e", "name-f", "name-dd", "name-ee", "name-ff" };
        characters.All(ch => correctCharactersInDatabase.Contains(ch.Name)).Should().BeTrue();

        correlations.Count.Should().Be(4);
        correlations.First(co => co.LoginCharacter.Name == "name-e").NumberOfMatches.Should().Be(2);
        correlations.First(co => co.LoginCharacter.Name == "name-d").NumberOfMatches.Should().Be(1);
        correlations.First(co => co.LoginCharacter.Name == "name-ee").NumberOfMatches.Should().Be(2);
        correlations.First(co => co.LoginCharacter.Name == "name-dd").NumberOfMatches.Should().Be(1);
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync<TestDatabaseSeederWorldScanAnalyser>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static async Task Subscribe(IWorldScansAnalyserSubscriber subscriber)
    {
        subscriber.Subscribe();
        await Task.Delay(DelayMilliseconds);
        subscriber.CloseChannels();
    }
}