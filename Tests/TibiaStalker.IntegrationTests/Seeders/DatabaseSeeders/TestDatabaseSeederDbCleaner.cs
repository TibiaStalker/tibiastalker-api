using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

public class TestDatabaseSeederDbCleaner : TestDatabaseSeeder
{
	private readonly ITibiaStalkerDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TestDatabaseSeederDbCleaner(ITibiaStalkerDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override async Task SeedDatabase()
    {
        await _dbContext.Worlds.AddRangeAsync(GetWorlds());
        await _dbContext.WorldScans.AddRangeAsync(GetWorldScans());
        await _dbContext.Characters.AddRangeAsync(GetCharacters());
        await _dbContext.CharacterCorrelations.AddRangeAsync(GetCharacterCorrelations());

        await _dbContext.SaveChangesAsync();
    }

    private static IEnumerable<World> GetWorlds()
    {
        return new List<World>
        {
            new() { WorldId = 11, Name = "World-a", IsAvailable = true, Url = "urlWorld-a" },
            new() { WorldId = 12, Name = "World-b", IsAvailable = false, Url = "urlWorld-b" }
        };
    }

    private static IEnumerable<WorldScan> GetWorldScans()
    {
        return new List<WorldScan>
        {
            new() { WorldScanId = 1110, WorldId = 11, ScanCreateDateTime = new DateTime(2022,11,30,20,23,12, DateTimeKind.Utc), CharactersOnline = "name-a|name-b|name-c|name-d|name-e|name-f|name-g|name-h"},
            new() { WorldScanId = 1111, WorldId = 11, ScanCreateDateTime = new DateTime(2022,11,30,20,28,36, DateTimeKind.Utc), CharactersOnline = "name-a|name-b|name-c|name-d|name-e|name-f|name-g|name-h"},
            new() { WorldScanId = 1112, WorldId = 11, ScanCreateDateTime = new DateTime(2022,11,30,20,33,52, DateTimeKind.Utc), CharactersOnline = "name-a|name-b|name-c|name-e|name-g|name-h|name-i"},
            new() { WorldScanId = 1113, WorldId = 11, ScanCreateDateTime = new DateTime(2022,11,30,20,39,06, DateTimeKind.Utc), CharactersOnline = "name-b|name-c|name-d|name-e|name-f|name-g|name-h"},
            new() { WorldScanId = 1114, WorldId = 11, ScanCreateDateTime = new DateTime(2022,11,30,20,55,02, DateTimeKind.Utc), CharactersOnline = "name-a|name-b|name-c|name-i|name-j|name-k|name-l|name-m"},

            new() { WorldScanId = 1210, WorldId = 12, ScanCreateDateTime = new DateTime(2022,11,30,20,23,12, DateTimeKind.Utc), CharactersOnline = "name-aa|name-bb|name-cc|name-dd|name-ee|name-ff|name-gg|name-hh" },
            new() { WorldScanId = 1211, WorldId = 12, ScanCreateDateTime = new DateTime(2022,11,30,20,28,36, DateTimeKind.Utc), CharactersOnline = "name-aa|name-bb|name-cc|name-dd|name-ee|name-ff|name-gg|name-hh" },
            new() { WorldScanId = 1212, WorldId = 12, ScanCreateDateTime = new DateTime(2022,11,30,20,33,52, DateTimeKind.Utc), CharactersOnline = "name-aa|name-bb|name-cc|name-ee|name-gg|name-hh|name-ii" },
            new() { WorldScanId = 1213, WorldId = 12, ScanCreateDateTime = new DateTime(2022,11,30,20,39,06, DateTimeKind.Utc), CharactersOnline = "name-bb|name-cc|name-dd|name-ee|name-ff|name-gg|name-hh" },
            new() { WorldScanId = 1224, WorldId = 12, ScanCreateDateTime = new DateTime(2022,11,30,21,00,18, DateTimeKind.Utc), CharactersOnline = "name-aa|name-bb|name-cc|name-ii|name-jj|name-kk|name-ll|name-mm" },
        };
    }

    private static IEnumerable<Character> GetCharacters()
    {
        return new List<Character>
        {
            new() {CharacterId = 120, WorldId = 11, Name = "name-a"},
            new() {CharacterId = 121, WorldId = 11, Name = "name-b"},
            new() {CharacterId = 122, WorldId = 11, Name = "name-c"},
            new() {CharacterId = 123, WorldId = 11, Name = "name-d"},
        };
    }

    private IEnumerable<CharacterCorrelation> GetCharacterCorrelations()
    {
        return new List<CharacterCorrelation>
        {
            new() { LoginCharacterId = 120, LogoutCharacterId = 121, NumberOfMatches = 1, LastMatchDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-40)},
            new() { LoginCharacterId = 120, LogoutCharacterId = 122, NumberOfMatches = 1, LastMatchDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-50) },
            new() { LoginCharacterId = 120, LogoutCharacterId = 123, NumberOfMatches = 1, LastMatchDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-20) },
            new() { LoginCharacterId = 121, LogoutCharacterId = 122, NumberOfMatches = 3, LastMatchDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-40) },
            new() { LoginCharacterId = 121, LogoutCharacterId = 123, NumberOfMatches = 6, LastMatchDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-20) },
            new() { LoginCharacterId = 122, LogoutCharacterId = 123, NumberOfMatches = 6, LastMatchDate = _dateTimeProvider.DateOnlyUtcNow.AddDays(-40) }
        };
    }
}