using Microsoft.EntityFrameworkCore;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

public abstract class TestDatabaseSeeder : ITestDatabaseSeeder
{
	private readonly ITibiaStalkerDbContext _dbContext;

	protected TestDatabaseSeeder(ITibiaStalkerDbContext dbContext)
	{
		_dbContext = dbContext;
	}

    public async Task ResetDatabaseAsync()
    {
        await ClearDatabaseAsync();
        await SeedDatabase();
    }

    protected abstract Task SeedDatabase();

    private async Task ClearDatabaseAsync()
    {
        var tableNames = _dbContext.WorldScans.EntityType.Model.GetEntityTypes().Select(t => t.GetTableName()).Distinct().ToList();

        foreach (var tableName in tableNames)
        {
            await _dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {tableName} CASCADE");
        }
    }
}