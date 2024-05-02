using Microsoft.EntityFrameworkCore;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace WorldScanSeeder;

public class ScanSeeder : IScanSeeder
{
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly ITibiaDataClient _tibiaDataClient;

    private List<World> _availableWorlds;

    public List<World> AvailableWorlds => _availableWorlds;

    public ScanSeeder(ITibiaStalkerDbContext dbContext, ITibiaDataClient tibiaDataClient)
    {
        _dbContext = dbContext;
        _tibiaDataClient = tibiaDataClient;
    }

    public async Task SetProperties()
    {
        _availableWorlds = await _dbContext.Worlds.Where(w => w.IsAvailable).ToListAsync();
    }

    public async Task Seed(World availableWorld)
    {
        var worldScan = await CreateWorldScanAsync(availableWorld);

        await _dbContext.WorldScans.AddAsync(worldScan);
        await _dbContext.SaveChangesAsync();
    }

    private async Task<WorldScan> CreateWorldScanAsync(World world)
    {
        var charactersOnline = await _tibiaDataClient.FetchCharactersOnline(world.Name);

        return new WorldScan
        {
            CharactersOnline = string.Join("|", charactersOnline).ToLower(),
            WorldId = world.WorldId,
            ScanCreateDateTime = DateTime.UtcNow,
        };
    }
}