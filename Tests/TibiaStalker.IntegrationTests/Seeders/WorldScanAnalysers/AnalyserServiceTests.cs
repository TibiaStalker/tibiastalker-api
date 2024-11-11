using FluentAssertions;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;
using WorldScanAnalyser;

namespace TibiaStalker.IntegrationTests.Seeders.WorldScanAnalysers;

[Collection(TestCollections.SeederCollection)]
public class AnalyserServiceTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;

    public AnalyserServiceTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnalyserService_Run_ShouldReturnCorrectDataAndSendMessagesToRabbit()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var analyser = scope.ServiceProvider.GetRequiredService<IAnalyserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await analyser.Run();

        var worldScans = dbContext.WorldScans.ToList();

        // Assert
        worldScans.Count.Should().Be(12);
        var softDeletedWorldScans = worldScans.Where(s => s.IsDeleted);
        softDeletedWorldScans.Count().Should().Be(10);
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