using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

public class TestDatabaseSeederWorldScanSeeder : TestDatabaseSeeder
{
	private readonly ITibiaStalkerDbContext _dbContext;

	public TestDatabaseSeederWorldScanSeeder(ITibiaStalkerDbContext dbContext) : base(dbContext)
	{
		_dbContext = dbContext;
	}

    protected override async Task SeedDatabase()
    {
        await _dbContext.Worlds.AddRangeAsync(GetWorlds());
        await _dbContext.SaveChangesAsync();
    }

    private static IEnumerable<World> GetWorlds()
    {
        return new List<World>
        {
            new() { WorldId = 11, Name = "World-a", IsAvailable = true, Url = "urlWorld-a" },
            new() { WorldId = 12, Name = "World-b", IsAvailable = true, Url = "urlWorld-b" },
            new() { WorldId = 13, Name = "World-c", IsAvailable = true, Url = "urlWorld-c" },
            new() { WorldId = 14, Name = "World-d", IsAvailable = true, Url = "urlWorld-d" },
            new() { WorldId = 15, Name = "World-e", IsAvailable = false, Url = "urlWorld-e" }
        };
    }
}