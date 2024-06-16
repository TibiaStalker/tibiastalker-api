using Microsoft.EntityFrameworkCore;
using Shared.Database.Queries.Sql;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;
using WorldScanAnalyser.Decorators;

namespace WorldScanAnalyser.Managers;

public class WorldScansProcessor
{
    private readonly ITibiaStalkerDbContext _dbContext1;
    private readonly ITibiaStalkerDbContext _dbContext2;
    private readonly IAnalyserLogDecorator _logDecorator;
    private List<int> _loginCharactersIds = new ();
    private List<int> _logoutCharactersIds = new ();
    private DateOnly _lastMatchDate;


    public WorldScansProcessor(ITibiaStalkerDbContext dbContext1,
        ITibiaStalkerDbContext dbContext2,
        IAnalyserLogDecorator logDecorator)
    {
        _dbContext1 = dbContext1;
        _dbContext2 = dbContext2;
        _logDecorator = logDecorator;
    }

    public async Task ProcessAsync(List<WorldScan> twoWorldScans)
    {
        await _logDecorator.Decorate(SeedCharacters, twoWorldScans);
        var updateOrCreateTask = UpdateOrCreate(twoWorldScans);
        var removeTask = _logDecorator.Decorate(RemoveImpossibleCorrelationsAsync, twoWorldScans);
        await Task.WhenAll(updateOrCreateTask, removeTask);
    }

    private async Task UpdateOrCreate(List<WorldScan> twoWorldScans)
    {
        await _logDecorator.Decorate(UpdateCorrelationsIfExistAsync, twoWorldScans);
        await _logDecorator.Decorate(CreateCorrelationsIfNotExistAsync, twoWorldScans);
    }

    private async Task SeedCharacters()
    {
        await _dbContext1.ExecuteRawSqlAsync(GenerateQueries.CreateCharactersIfNotExists);
    }

    private async Task UpdateCorrelationsIfExistAsync()
    {
        _lastMatchDate = _dbContext1.CharacterActions.First().LogoutOrLoginDate;
        _loginCharactersIds = GetCharactersIdsBasedOnCharacterActions(isOnline: true);
        _logoutCharactersIds = GetCharactersIdsBasedOnCharacterActions(isOnline: false);

        var characterCorrelationsIdsPart1 = _dbContext1.CharacterCorrelations
            .Where(c => _loginCharactersIds.Contains(c.LoginCharacterId) && _logoutCharactersIds.Contains(c.LogoutCharacterId))
            .Select(cc => cc.CorrelationId);

        var characterCorrelationsIdsPart2 = _dbContext1.CharacterCorrelations
            .Where(c => _logoutCharactersIds.Contains(c.LoginCharacterId) && _loginCharactersIds.Contains(c.LogoutCharacterId))
            .Select(cc => cc.CorrelationId);

        var characterCorrelationsIds = characterCorrelationsIdsPart1.Concat(characterCorrelationsIdsPart2).ToList();

        await _dbContext1.CharacterCorrelations
            .Where(cc => characterCorrelationsIds.Contains(cc.CorrelationId))
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.NumberOfMatches, c => c.NumberOfMatches + 1)
                .SetProperty(c => c.LastMatchDate, _lastMatchDate));
    }

    private async Task CreateCorrelationsIfNotExistAsync()
    {
        var correlationsDataToCreate = _loginCharactersIds
            .SelectMany(login => _logoutCharactersIds, (login, logout) => new { LoginCharacterId = login, LogoutCharacterId = logout });

        var existingCharacterCorrelationsPart1 = _dbContext1.CharacterCorrelations
            .Where(c => _loginCharactersIds.Contains(c.LoginCharacterId) && _logoutCharactersIds.Contains(c.LogoutCharacterId))
            .Select(cc => new { LoginCharacterId = cc.LoginCharacterId, LogoutCharacterId = cc.LogoutCharacterId });

        var existingCharacterCorrelationsPart2 = _dbContext1.CharacterCorrelations
            .Where(c => _logoutCharactersIds.Contains(c.LoginCharacterId) && _loginCharactersIds.Contains(c.LogoutCharacterId))
            .Select(cc => new { LoginCharacterId = cc.LogoutCharacterId, LogoutCharacterId = cc.LoginCharacterId });

        var existingCharacterCorrelations = existingCharacterCorrelationsPart1.Concat(existingCharacterCorrelationsPart2).ToList();
        var correlationsDataToInsert = correlationsDataToCreate.Except(existingCharacterCorrelations).ToList();

        var newCorrelations = correlationsDataToInsert
            .Select(cc => new CharacterCorrelation
            {
                LoginCharacterId = cc.LoginCharacterId,
                LogoutCharacterId = cc.LogoutCharacterId,
                NumberOfMatches = 1,
                CreateDate = _lastMatchDate,
                LastMatchDate = _lastMatchDate
            }).ToList();

        if (newCorrelations.Any())
        {
            await _dbContext1.CharacterCorrelations.AddRangeAsync(newCorrelations);
            await _dbContext1.SaveChangesAsync();
        }
    }

    private async Task RemoveImpossibleCorrelationsAsync()
    {
        await _dbContext2.ExecuteRawSqlAsync(GenerateQueries.RemoveImpossibleCorrelations);
        // TODO: update tests for that method
    }

    private List<int> GetCharactersIdsBasedOnCharacterActions(bool isOnline)
    {
        var characterNames = _dbContext1.CharacterActions
            .Where(ca => ca.IsOnline == isOnline)
            .Select(ca => ca.CharacterName)
            .Distinct();

        var currentIds = _dbContext1.Characters
            .Where(c => characterNames.Contains(c.Name))
            .Select(c => c.CharacterId)
            .ToList();

        return currentIds;
    }
}