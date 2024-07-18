using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

public class TestDatabaseSeederCharacterNameDetector : TestDatabaseSeeder
{
    private readonly ITibiaStalkerDbContext _dbContext;

    public TestDatabaseSeederCharacterNameDetector(ITibiaStalkerDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task SeedDatabase()
    {
        await _dbContext.Worlds.AddRangeAsync(GetWorlds());
        await _dbContext.Characters.AddRangeAsync(GetCharacters());
        await _dbContext.CharacterCorrelations.AddRangeAsync(GetCharacterCorrelations());
        await _dbContext.SaveChangesAsync();
    }

    private static IEnumerable<World> GetWorlds()
    {
        return new List<World>
        {
            new() { WorldId = 11, Name = "World-a", IsAvailable = true, Url = "urlWorld-a" },
            new() { WorldId = 12, Name = "World-b", IsAvailable = true, Url = "urlWorld-b" },
        };
    }

    private IEnumerable<Character> GetCharacters()
    {
        return new List<Character>
        {
            new() { CharacterId = 111, Name = "name-d", WorldId = 11 },
            new() { CharacterId = 112, Name = "name-e", WorldId = 11, TradedDate = DateOnly.FromDateTime(DateTime.Now), VerifiedDate = DateOnly.FromDateTime(DateTime.Now) },
            new() { CharacterId = 113, Name = "name-f", WorldId = 11, TradedDate = DateOnly.FromDateTime(DateTime.Now), VerifiedDate = DateOnly.FromDateTime(DateTime.Now) },
            new() { CharacterId = 114, Name = "name-g", WorldId = 11, DeleteApproachNumber = 4 }
        };
    }

    private IEnumerable<CharacterCorrelation> GetCharacterCorrelations()
    {
        return new List<CharacterCorrelation>
        {
            new()
            {
                CorrelationId = 1111, LoginCharacterId = 111, LogoutCharacterId = 112,
                CreateDate = DateTodayMinusDays(10), LastMatchDate = DateTodayMinusDays(5), NumberOfMatches = 5
            },
            new()
            {
                CorrelationId = 1112, LoginCharacterId = 112, LogoutCharacterId = 113,
                CreateDate = DateTodayMinusDays(100), LastMatchDate = DateTodayMinusDays(50), NumberOfMatches = 15
            },
            new()
            {
                CorrelationId = 1113, LoginCharacterId = 114, LogoutCharacterId = 113,
                CreateDate = DateTodayMinusDays(20), LastMatchDate = DateTodayMinusDays(25), NumberOfMatches = 10
            }
        };
    }

    private static DateOnly DateTodayMinusDays(int days)
    {
        return DateOnly.FromDateTime(DateTime.Today).AddDays(-days);
    }
}