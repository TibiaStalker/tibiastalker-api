using Microsoft.EntityFrameworkCore;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace WorldScanAnalyser.Managers;

public class CharacterActionsManager
{
    private IReadOnlyList<string> _loginNames;
    private IReadOnlyList<string> _logoutNames;
    private IReadOnlyList<string> _firstScanNames;
    private IReadOnlyList<string> _secondScanNames;
    private List<CharacterAction> _loginCharacters;
    private List<CharacterAction> _logoutCharacters;

    private readonly ITibiaStalkerDbContext _dbContext1;
    private readonly ITibiaStalkerDbContext _dbContext2;

    public CharacterActionsManager(ITibiaStalkerDbContext dbContext1, ITibiaStalkerDbContext dbContext2)
    {
        _dbContext1 = dbContext1;
        _dbContext2 = dbContext2;
    }

    public async Task SeedCharacterActions(List<WorldScan> twoWorldScans)
    {
        var addTask = AddCharacterActions(twoWorldScans);
        var setTask = SetFindInScan();

        await Task.WhenAll(addTask, setTask);
    }

    private async Task SetFindInScan()
    {
        await _dbContext2.Characters
            .Where(ch => _firstScanNames.Contains(ch.Name) || _secondScanNames.Contains(ch.Name))
            .ExecuteUpdateAsync(update => update
                .SetProperty(
                    c => c.FoundInScan1,
                    c => _firstScanNames.Contains(c.Name) || c.FoundInScan1)
                .SetProperty(
                    c => c.FoundInScan2,
                    c => _secondScanNames.Contains(c.Name) || c.FoundInScan2)
            );
    }

    private async Task AddCharacterActions(List<WorldScan> twoWorldScans)
    {
        var setLoginCharacters = SetLoginCharacters(_loginNames, twoWorldScans[1], isOnline: true);
        var setLogoutCharacters = SetLogoutCharacters(_logoutNames, twoWorldScans[0], isOnline: false);
        await Task.WhenAll(setLogoutCharacters, setLoginCharacters);

        var add1 = _dbContext1.CharacterActions.AddRangeAsync(_loginCharacters);
        var add2 = _dbContext1.CharacterActions.AddRangeAsync(_logoutCharacters);
        await Task.WhenAll(add1, add2);
        await _dbContext1.SaveChangesAsync();
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

    private async Task SetLoginCharacters(IEnumerable<string> names, WorldScan worldScan, bool isOnline)
    {
        await Task.FromResult(_loginCharacters = CreateCharactersActions(names, worldScan, isOnline));
    }

    private async Task SetLogoutCharacters(IEnumerable<string> names, WorldScan worldScan, bool isOnline)
    {
        await Task.FromResult(_logoutCharacters = CreateCharactersActions(names, worldScan, isOnline));
    }

    private List<CharacterAction> CreateCharactersActions(IEnumerable<string> names, WorldScan worldScan, bool isOnline)
    {
        var characterActions = names.Select(name => new CharacterAction()
        {
            CharacterName = name,
            WorldScanId = worldScan.WorldScanId,
            WorldId = worldScan.WorldId,
            IsOnline = isOnline,
            LogoutOrLoginDate = DateOnly.FromDateTime(worldScan.ScanCreateDateTime)
        }).ToList();

        return characterActions;
    }

    private static List<string> GetNames(WorldScan worldScan)
    {
        return worldScan.CharactersOnline.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}