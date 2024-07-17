using Microsoft.EntityFrameworkCore;
using Shared.Database.Queries.Sql;
using TibiaStalker.Infrastructure.Persistence;

namespace DbCleaner;

public class Cleaner : ICleaner
{
    private const int Minute = 60;
    private readonly ITibiaStalkerDbContext _dbContext;

    public Cleaner(ITibiaStalkerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ClearUnnecessaryWorldScans()
    {
        await _dbContext.WorldScans
            .Where(ws => !ws.World.IsAvailable)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteIrrelevantCharacterCorrelations()
    {
        var thresholdDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

        await _dbContext.CharacterCorrelations
            .Where(c => c.NumberOfMatches < 3 && c.LastMatchDate < thresholdDate)
            .ExecuteDeleteAsync();
    }

    public async Task VacuumWorldScans()
    {
        await _dbContext.ExecuteRawSqlAsync("VACUUM FULL world_scans", timeOut: 1 * Minute);
    }

    public async Task VacuumCharacters()
    {
        await _dbContext.ExecuteRawSqlAsync("VACUUM FULL characters", timeOut: 10 * Minute);
    }

    public async Task VacuumWorlds()
    {
        await _dbContext.ExecuteRawSqlAsync("VACUUM FULL worlds", timeOut: 1 * Minute);
    }

    public async Task VacuumCharacterCorrelations()
    {
        await _dbContext.ExecuteRawSqlAsync("VACUUM FULL character_correlations", timeOut: 30 * Minute);
    }
}