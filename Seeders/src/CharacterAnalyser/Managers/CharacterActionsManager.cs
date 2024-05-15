using Microsoft.EntityFrameworkCore;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace CharacterAnalyser.Managers;

public class CharacterActionsManager
{
    private IReadOnlyList<string> _loginNames;
    private IReadOnlyList<string> _logoutNames;
    private IReadOnlyList<string> _firstScanNames;
    private IReadOnlyList<string> _secondScanNames;

    private readonly ITibiaStalkerDbContext _dbContext;

    public CharacterActionsManager(ITibiaStalkerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedCharacterActions(List<WorldScan> twoWorldScans)
    {
        var logoutCharacters = CreateCharactersActionsAsync(_logoutNames, twoWorldScans[0], isOnline: false);
        var loginCharacters = CreateCharactersActionsAsync(_loginNames, twoWorldScans[1], isOnline: true);

        var characterActionsToAdd = logoutCharacters.Concat(loginCharacters);
        await _dbContext.CharacterActions.AddRangeAsync(characterActionsToAdd);
        await _dbContext.SaveChangesAsync();

        await _dbContext.Characters
            .Where(ch => _firstScanNames.Contains(ch.Name))
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.FoundInScan1, c => true));

        await _dbContext.Characters
            .Where(ch => _secondScanNames.Contains(ch.Name))
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.FoundInScan2, c => true));
    }

    public void SetFirstAndSecondScanNames(List<WorldScan> twoWorldScans)
    {
        _firstScanNames = GetNames(twoWorldScans[0]);
        _secondScanNames = GetNames(twoWorldScans[1]);
    }

    public IReadOnlyList<string> GetAndSetLoginNames()
    {
        return _loginNames = _secondScanNames.Except(_firstScanNames).ToArray();
    }

    public IReadOnlyList<string> GetAndSetLogoutNames()
    {
        return _logoutNames = _firstScanNames.Except(_secondScanNames).ToArray();
    }

    private static List<CharacterAction> CreateCharactersActionsAsync(IEnumerable<string> names, WorldScan worldScan, bool isOnline)
    {
        var logoutOrLoginDate = DateOnly.FromDateTime(worldScan.ScanCreateDateTime);
        
        return names.Select(name => new CharacterAction()
        {
            CharacterName = name,
            WorldScanId = worldScan.WorldScanId,
            WorldId = worldScan.WorldId,
            IsOnline = isOnline,
            LogoutOrLoginDate = logoutOrLoginDate
        }).ToList();
    }

    private static List<string> GetNames(WorldScan worldScan)
    {
        return worldScan.CharactersOnline.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}