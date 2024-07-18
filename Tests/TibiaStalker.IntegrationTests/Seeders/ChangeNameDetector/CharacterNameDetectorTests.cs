using ChangeNameDetector.Services;
using FluentAssertions;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

namespace TibiaStalker.IntegrationTests.Seeders.ChangeNameDetector;

[Collection(TestCollections.SeederCollection)]
public class CharacterNameDetectorTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;

    public CharacterNameDetectorTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ChangeNameDetector_Run_ShouldChangeAllCharacterVerifiedDateToDataNow()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await changeNameDetector.Run();

        var characters = dbContext.Characters.ToList();

        // Assert
        characters.All(ch => ch.VerifiedDate == DateOnly.FromDateTime(DateTime.Now)).Should().BeTrue();
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync<TestDatabaseSeederWorldScanAnalyser>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}