using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Infrastructure.Configuration;

public class ClearTrackedCharactersInitializer : IInitializer
{
    private readonly ILogger<ClearTrackedCharactersInitializer> _logger;
    private readonly ITibiaStalkerDbContext _dbContext;

    public ClearTrackedCharactersInitializer(ILogger<ClearTrackedCharactersInitializer> logger, ITibiaStalkerDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public int? Order => null;

    public async Task Initialize()
    {
        try
        {
            _logger.LogInformation("Clear TrackedCharacters table initializer - started");
            await _dbContext.TrackedCharacters.ExecuteDeleteAsync();
            _logger.LogInformation("Clear TrackedCharacters table initializer  - finished");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while cleaning TrackedCharacters table");
            Log.CloseAndFlush();
            throw;
        }
    }
}
