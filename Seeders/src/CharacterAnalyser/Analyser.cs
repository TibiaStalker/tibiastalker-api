using System.Diagnostics;
using CharacterAnalyser.ActionRules;
using CharacterAnalyser.ActionRules.Rules;
using CharacterAnalyser.Decorators;
using CharacterAnalyser.Managers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Database.Queries.Sql;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace CharacterAnalyser;

public class Analyser : ActionRule, IAnalyser
{
    private readonly ITibiaStalkerDbContext _dbContext1;
    private readonly ITibiaStalkerDbContext _dbContext2;
    private readonly IAnalyserLogDecorator _logDecorator;
    private readonly ILogger<Analyser> _logger;
    private readonly CharacterActionsManager _characterActionsManager;
    private readonly WorldScansProcessor _processor;

    public Analyser(ITibiaStalkerDbContext dbContext1,
        ITibiaStalkerDbContext dbContext2,
        IAnalyserLogDecorator logDecorator,
        ILogger<Analyser> logger)
    {
        _dbContext1 = dbContext1;
        _dbContext2 = dbContext2;
        _logDecorator = logDecorator;
        _logger = logger;
        _characterActionsManager = new CharacterActionsManager(dbContext1, dbContext2);
        _processor = new WorldScansProcessor(dbContext1, dbContext2, logDecorator);
    }

    public List<short> GetDistinctWorldIdsFromRemainingScans()
    {
        var result = _dbContext1.WorldScans
            .Where(scan => !scan.IsDeleted)
            .Select(scan => new { scan.WorldId })
            .GroupBy(scan => scan.WorldId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id)
            .ToList();

        return result;
    }

    public List<WorldScan> GetWorldScansToAnalyse(short worldId)
    {
        var result = _dbContext1.WorldScans
            .Where(scan => scan.WorldId == worldId && !scan.IsDeleted)
            .OrderBy(scan => scan.ScanCreateDateTime)
            .Take(2)
            .AsNoTracking()
            .ToList();

        return result;
    }

    public async Task Seed(List<WorldScan> twoWorldScans)
    {
        var stopwatch = Stopwatch.StartNew();

        if (IsBroken(new NumberOfWorldScansShouldBe2Rule(twoWorldScans)))
            return;

        if (IsBroken(new TimeBetweenWorldScansCannotBeLongerThanMaxDurationRule(twoWorldScans)))
        {
            SoftDeleteWorldScan(twoWorldScans[0].WorldScanId);
            return;
        }

        _characterActionsManager.SetFirstAndSecondScanNames(twoWorldScans);

        if (IsBroken(new CharacterNameListCannotBeEmptyRule(_characterActionsManager.GetAndSetLogoutNames())) ||
            IsBroken(new CharacterNameListCannotBeEmptyRule(_characterActionsManager.GetAndSetLoginNames())))
        {
            SoftDeleteWorldScan(twoWorldScans[0].WorldScanId);
            return;
        }

        await Task.WhenAll(_dbContext1.ExecuteRawSqlAsync(GenerateQueries.ClearCharacterActions),
            _dbContext2.ExecuteRawSqlAsync(GenerateQueries.ResetCharacterFoundInScans));

        _logger.LogInformation("Prepare everything to analyse, execution time : {time} ms.", stopwatch.ElapsedMilliseconds);

        try
        {
            await _logDecorator.Decorate(_characterActionsManager.SeedCharacterActions, twoWorldScans);
            await _processor.ProcessAsync(twoWorldScans);
        }
        finally
        {
            SoftDeleteWorldScan(twoWorldScans[0].WorldScanId);
            _dbContext1.ChangeTracker.Clear();
            _dbContext2.ChangeTracker.Clear();
        }
    }

    private void SoftDeleteWorldScan(int scanId)
    {
        _dbContext1.WorldScans
            .Where(ws => ws.WorldScanId == scanId)
            .ExecuteUpdate(update => update.SetProperty(ws => ws.IsDeleted, true));
    }
}