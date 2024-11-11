using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

public class TestDatabaseSeederWorldScanAnalyser : TestDatabaseSeeder
{
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TestDatabaseSeederWorldScanAnalyser(ITibiaStalkerDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task SeedDatabase()
    {
        await _dbContext.Worlds.AddRangeAsync(GetWorlds());
        await _dbContext.WorldScans.AddRangeAsync(GetWorldScans());
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

    private IEnumerable<WorldScan> GetWorldScans()
    {
        return new List<WorldScan>
        {
            new()
            {
                WorldScanId = 110, IsDeleted = false, WorldId = 11,
                ScanCreateDateTime = TimeNowPlusMinutes(0),
                CharactersOnline = "name-a|name-b|name-c|name-d"
            },
            new()
            {
                WorldScanId = 111, IsDeleted = false, WorldId = 11,
                ScanCreateDateTime = TimeNowPlusMinutes(0),
                CharactersOnline = "name-a|name-b|name-c|name-d"
            },
            new()
            {
                WorldScanId = 112, IsDeleted = false, WorldId = 11,
                ScanCreateDateTime = TimeNowPlusMinutes(5),
                CharactersOnline = "name-a|name-b|name-c|name-e"
            }, // d - e
            new()
            {
                WorldScanId = 113, IsDeleted = false, WorldId = 11,
                ScanCreateDateTime = TimeNowPlusMinutes(10),
                CharactersOnline = "name-a|name-b|name-c|name-e|name-f"
            }, //  - f
            new()
            {
                WorldScanId = 114, IsDeleted = false, WorldId = 11,
                ScanCreateDateTime = TimeNowPlusMinutes(16),
                CharactersOnline = "name-a|name-b|name-c|name-d"
            }, // e, f - d
            new()
            {
                WorldScanId = 115, IsDeleted = false, WorldId = 11,
                ScanCreateDateTime = TimeNowPlusMinutes(60),
                CharactersOnline = "name-a|name-b|name-c|name-h"
            },


            new()
            {
                WorldScanId = 220, IsDeleted = false, WorldId = 12,
                ScanCreateDateTime = TimeNowPlusMinutes(0),
                CharactersOnline = "name-aa|name-bb|name-cc|name-dd"
            },
            new()
            {
                WorldScanId = 221, IsDeleted = false, WorldId = 12,
                ScanCreateDateTime = TimeNowPlusMinutes(0),
                CharactersOnline = "name-aa|name-bb|name-cc|name-dd"
            },
            new()
            {
                WorldScanId = 222, IsDeleted = false, WorldId = 12,
                ScanCreateDateTime = TimeNowPlusMinutes(5),
                CharactersOnline = "name-aa|name-bb|name-cc|name-ee"
            }, // dd - ee
            new()
            {
                WorldScanId = 223, IsDeleted = false, WorldId = 12,
                ScanCreateDateTime = TimeNowPlusMinutes(10),
                CharactersOnline = "name-aa|name-bb|name-cc|name-ee|name-ff"
            }, // - ff
            new()
            {
                WorldScanId = 224, IsDeleted = false, WorldId = 12,
                ScanCreateDateTime = TimeNowPlusMinutes(16),
                CharactersOnline = "name-aa|name-bb|name-cc|name-dd"
            }, // ee, ff - dd
            new()
            {
                WorldScanId = 225, IsDeleted = false, WorldId = 12,
                ScanCreateDateTime = TimeNowPlusMinutes(60),
                CharactersOnline = "name-aa|name-bb|name-cc|name-hh"
            }
        };
    }

    private DateTime TimeNowPlusMinutes(int minutes)
    {
        return _dateTimeProvider.DateTimeUtcNow.AddMinutes(minutes);
    }
}