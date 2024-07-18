using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Configuration;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests.API.DatabaseSeeders;

public class TestDatabaseApiInitializer : IInitializer
{
	private readonly ITibiaStalkerDbContext _dbContext;

	public TestDatabaseApiInitializer(ITibiaStalkerDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public int? Order => 2;
	public async Task Initialize()
	{
		if (_dbContext.Characters.FirstOrDefault() is null)
		{
			await SeedDatabase();
		}
	}

	private async Task SeedDatabase()
	{
		await _dbContext.Worlds.AddRangeAsync(CreateWorlds());
		await _dbContext.Characters.AddRangeAsync(CreateCharacters());
		await _dbContext.CharacterCorrelations.AddRangeAsync(CreateCharacterCorrelations());

		await _dbContext.SaveChangesAsync();
	}

    private static IEnumerable<World> CreateWorlds()
    {
        return new List<World>()
        {
            new() { WorldId = 1, Name = "World-A", Url = "urlWorld-A", IsAvailable = true },
            new() { WorldId = 2, Name = "World-B", Url = "urlWorld-B", IsAvailable = true },
            new() { WorldId = 3, Name = "World-C", Url = "urlWorld-C", IsAvailable = false }
        };
    }

    private static IEnumerable<Character> CreateCharacters()
    {
        return new List<Character>()
        {
            new() { CharacterId = 1, Name = "name-a", WorldId = 1 },
            new() { CharacterId = 2, Name = "name-b", WorldId = 1 },
            new() { CharacterId = 3, Name = "name-c", WorldId = 1 },

            new() { CharacterId = 4, Name = "name-d", WorldId = 2 },
            new() { CharacterId = 5, Name = "name-e", WorldId = 2 },
            new() { CharacterId = 6, Name = "name-f", WorldId = 2 },
        };
    }

    private static IEnumerable<CharacterCorrelation> CreateCharacterCorrelations()
    {
        return new List<CharacterCorrelation>()
        {
            new() { LoginCharacterId = 1, LogoutCharacterId = 2, NumberOfMatches = 8 },
            new() { LoginCharacterId = 3, LogoutCharacterId = 1, NumberOfMatches = 4 },
            new() { LoginCharacterId = 3, LogoutCharacterId = 2, NumberOfMatches = 2 },

            new() { LoginCharacterId = 4, LogoutCharacterId = 6, NumberOfMatches = 10 },
            new() { LoginCharacterId = 6, LogoutCharacterId = 5, NumberOfMatches = 21 },
            new() { LoginCharacterId = 5, LogoutCharacterId = 6, NumberOfMatches = 1 },
        };
    }
}