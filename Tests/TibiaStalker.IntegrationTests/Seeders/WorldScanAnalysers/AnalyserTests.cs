using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;
using WorldScanAnalyser;

namespace TibiaStalker.IntegrationTests.Seeders.WorldScanAnalysers;

[Collection(TestCollections.SeederCollection)]
public class AnalyserTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;

    public AnalyserTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Analyser_GetDistinctWorldIdsFromRemainingScans_ShouldReturnCorrectData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var analyser = scope.ServiceProvider.GetRequiredService<IAnalyser>();

        // Act
        var worldsIds = analyser.GetDistinctWorldIdsFromRemainingScans();

        // Assert
        worldsIds.Count.Should().Be(2);
        worldsIds[0].Should().Be(11);
        worldsIds[1].Should().Be(12);
    }

    [Fact]
    public void Analyser_GetWorldScansToAnalyse_ShouldReturnCorrectData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var analyser = scope.ServiceProvider.GetRequiredService<IAnalyser>();
        const short worldId = 11;

        // Act
        var worldScans = analyser.GetTwoWorldScansToAnalyse(worldId);

        // Assert
        worldScans.Count.Should().Be(2);
    }

    [Fact]
    public async Task Analyser_SoftDeleteWorldScanAsync_ShouldChangeIsDeletedToTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var analyser = scope.ServiceProvider.GetRequiredService<IAnalyser>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        const int worldScanId = 115;

        // Act
        await analyser.SoftDeleteWorldScanAsync(worldScanId);
        var worldScan = await dbContext.WorldScans.FirstOrDefaultAsync(s => s.WorldScanId == worldScanId);

        // Assert
        worldScan.Should().NotBeNull();
        worldScan!.IsDeleted.Should().BeTrue();
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