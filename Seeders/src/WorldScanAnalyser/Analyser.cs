using Microsoft.EntityFrameworkCore;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;
using WorldScanAnalyser.ActionRules;

namespace WorldScanAnalyser;

public class Analyser : ActionRule, IAnalyser
{
    private readonly ITibiaStalkerDbContext _dbContext;

    public Analyser(ITibiaStalkerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<short> GetDistinctWorldIdsFromRemainingScans()
    {
        var result = _dbContext.WorldScans
            .Where(scan => !scan.IsDeleted)
            .Select(scan => new { scan.WorldId })
            .GroupBy(scan => scan.WorldId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(id => id)
            .ToList();

        return result;
    }

    public List<WorldScan> GetTwoWorldScansToAnalyse(short worldId)
    {
        var result = _dbContext.WorldScans
            .Where(scan => scan.WorldId == worldId && !scan.IsDeleted)
            .OrderBy(scan => scan.ScanCreateDateTime)
            .Take(2)
            .AsNoTracking()
            .ToList();

        return result;
    }
    public async Task SoftDeleteWorldScanAsync(int scanId)
    {
        await _dbContext.WorldScans
            .Where(ws => ws.WorldScanId == scanId)
            .ExecuteUpdateAsync(update => update.SetProperty(ws => ws.IsDeleted, true));
    }
}