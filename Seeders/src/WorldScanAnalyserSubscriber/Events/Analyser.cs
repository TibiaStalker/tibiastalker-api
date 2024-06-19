using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;
using WorldScanAnalyserSubscriber.Events.ActionRules;
using WorldScanAnalyserSubscriber.Events.ActionRules.Rules;
using WorldScanAnalyserSubscriber.Events.Decorators;

namespace WorldScanAnalyserSubscriber.Events;

public class Analyser : ActionRule, IAnalyser
{
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly IAnalyserLogDecorator _logDecorator;
    private readonly ILogger<Analyser> _logger;

    private IReadOnlyList<string> _loginNames;
    private IReadOnlyList<string> _logoutNames;
    private IReadOnlyList<string> _firstScanNames;
    private IReadOnlyList<string> _secondScanNames;

    private List<int> _loginCharactersIds = new();
    private List<int> _logoutCharactersIds = new();

    public Analyser(ITibiaStalkerDbContext dbContext,
        IAnalyserLogDecorator logDecorator,
        ILogger<Analyser> logger)
    {
        _dbContext = dbContext;
        _logDecorator = logDecorator;
        _logger = logger;
    }

    public async Task Seed(WorldScan[] worldScans)
    {
        var stopwatch = Stopwatch.StartNew();

        if (IsBroken(new TimeBetweenWorldScansCannotBeLongerThanMaxDurationRule(worldScans)))
        {
            _logger.LogInformation("{method}. WorldScans({worldScanId1}/{worldScanId2}) - World({worldId}). Execution time: {time} ms",
                nameof(TimeBetweenWorldScansCannotBeLongerThanMaxDurationRule),
                worldScans[0].WorldScanId, worldScans[1].WorldScanId, worldScans[0].WorldId, stopwatch.ElapsedMilliseconds);

            return;
        }

        SetProperties(worldScans);

        if (_logoutNames.Count == 0)
        {
            _logger.LogInformation("{metod} empty. WorldScans({worldScanId1}/{worldScanId2}) - World({worldId}). Execution time: {time} ms",
                nameof(_logoutNames), worldScans[0].WorldScanId, worldScans[1].WorldScanId, worldScans[0].WorldId, stopwatch.ElapsedMilliseconds);
            return;
        }
        if (_loginNames.Count == 0)
        {
            _logger.LogInformation("{metod} empty. WorldScans({worldScanId1}/{worldScanId2}) - World({worldId}). Execution time: {time} ms",
                nameof(_loginNames), worldScans[0].WorldScanId, worldScans[1].WorldScanId, worldScans[0].WorldId, stopwatch.ElapsedMilliseconds);
            return;
        }

        await _logDecorator.Decorate(SeedCharacters, worldScans);
        await _logDecorator.Decorate(UpdateOrCreateCorrelationsAsync, worldScans);
        await _logDecorator.Decorate(RemoveImpossibleCorrelationsAsync, worldScans);

        await _dbContext.WorldScans.Where(ws => ws.WorldScanId == worldScans[0].WorldScanId).ExecuteDeleteAsync();
        _dbContext.ChangeTracker.Clear();
    }

    private void SetProperties(WorldScan[] worldScans)
    {
        _firstScanNames = GetNames(worldScans[0]);
        _secondScanNames = GetNames(worldScans[1]);
        _logoutNames = _firstScanNames.Except(_secondScanNames).ToArray();
        _loginNames = _secondScanNames.Except(_firstScanNames).ToArray();
    }

    private static List<string> GetNames(WorldScan worldScan)
    {
        return worldScan.CharactersOnline.Split("|", StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private async Task SeedCharacters(WorldScan[] worldScans)
    {
        var worldId = worldScans[0].WorldId;
        var names = new List<string>();
        names.AddRange(_loginNames);
        names.AddRange(_logoutNames);

        var existingNames = _dbContext.Characters
            .Where(c => names.Contains(c.Name))
            .Select(c => c.Name)
            .ToList();

        var newNames = names.Except(existingNames).ToList();

        foreach (var name in newNames)
        {
            _dbContext.Characters.Add(new Character { Name = name, WorldId = worldId });
        }

        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateOrCreateCorrelationsAsync(WorldScan[] worldScans)
    {
        var lastMatchDate = DateOnly.FromDateTime(worldScans[0].ScanCreateDateTime);
        _loginCharactersIds = GetCharactersIdsBasedOnNames(_loginNames);
        _logoutCharactersIds = GetCharactersIdsBasedOnNames(_logoutNames);

        var characterCorrelations = await _dbContext.CharacterCorrelations
            .Where(c => (_loginCharactersIds.Contains(c.LoginCharacterId) && _logoutCharactersIds.Contains(c.LogoutCharacterId)) ||
                        (_logoutCharactersIds.Contains(c.LoginCharacterId) && _loginCharactersIds.Contains(c.LogoutCharacterId)))
            .ToListAsync();

        if (characterCorrelations.Count != 0)
        {
            characterCorrelations.ForEach(cc =>
            {
                cc.NumberOfMatches += 1;
                cc.LastMatchDate = lastMatchDate;
            });

            await _dbContext.SaveChangesAsync();
        }

        var existingCorrelationSet = new HashSet<(int LoginCharacterId, int LogoutCharacterId)>(
            characterCorrelations.Select(cc => (cc.LoginCharacterId, cc.LogoutCharacterId))
                .Concat(characterCorrelations.Select(cc => (cc.LogoutCharacterId, cc.LoginCharacterId)))
        );

        var newCorrelations = (
            from login in _loginCharactersIds
            from logout in _logoutCharactersIds
            where !existingCorrelationSet.Contains((login, logout))
            select new CharacterCorrelation
            {
                LoginCharacterId = login,
                LogoutCharacterId = logout,
                NumberOfMatches = 1,
                CreateDate = lastMatchDate,
                LastMatchDate = lastMatchDate
            }).ToList();

        if (newCorrelations.Count != 0)
        {
            await _dbContext.CharacterCorrelations.AddRangeAsync(newCorrelations);
            await _dbContext.SaveChangesAsync();
        }
    }

    private async Task RemoveImpossibleCorrelationsAsync()
    {
        var secondScanIds = GetCharactersIdsBasedOnNames(_secondScanNames);
        var restIds = GetCharactersIdsBasedOnNames(_secondScanNames.Except(_firstScanNames));

        await _dbContext.CharacterCorrelations
            .Where(c => (secondScanIds.Contains(c.LoginCharacterId) && restIds.Contains(c.LogoutCharacterId)) ||
                        (restIds.Contains(c.LoginCharacterId) && secondScanIds.Contains(c.LogoutCharacterId)))
            .ExecuteDeleteAsync();
    }

    private List<int> GetCharactersIdsBasedOnNames(IEnumerable<string> names)
    {
        var currentIds = _dbContext.Characters
            .Where(c => names.Contains(c.Name))
            .Select(c => c.CharacterId)
            .ToList();

        return currentIds;
    }
}