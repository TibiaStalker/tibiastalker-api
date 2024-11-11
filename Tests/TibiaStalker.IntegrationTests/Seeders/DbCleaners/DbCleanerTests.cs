using DbCleaner;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

namespace TibiaStalker.IntegrationTests.Seeders.DbCleaners;

[Collection(TestCollections.SeederCollection)]
public class DbCleanerTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;

    public DbCleanerTests(TibiaSeederFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task DbCleaner_ClearUnnecessaryWorldScans_ShouldDeleteScansThatWorldIsNoLongerAvailable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cleaner = scope.ServiceProvider.GetRequiredService<ICleaner>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await cleaner.ClearUnnecessaryWorldScans();

        // Assert
        var worldScans = dbContext.WorldScans.AsNoTracking().ToList();

        worldScans.Count.Should().Be(5);
    }

    [Fact]
    public async Task DbCleaner_DeleteIrrelevantCharacterCorrelations_ShouldDeleteIrrelevantCorrelations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cleaner = scope.ServiceProvider.GetRequiredService<ICleaner>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();

        // Act
        await cleaner.DeleteIrrelevantCharacterCorrelations();

        // Assert
        var characterCorrelations = dbContext.CharacterCorrelations.AsNoTracking().ToList();

        characterCorrelations.Count.Should().Be(4);
    }
    
    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync<TestDatabaseSeederDbCleaner>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}