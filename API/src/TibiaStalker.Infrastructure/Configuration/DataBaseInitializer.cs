using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Infrastructure.Configuration;

public class DataBaseInitializer : IInitializer
{
    private readonly ILogger<DataBaseInitializer> _logger;
    private readonly ITibiaStalkerDbContext _dbContext;

    public DataBaseInitializer(ILogger<DataBaseInitializer> logger, ITibiaStalkerDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public int? Order => 1;

    public async Task Initialize()
    {
        try
        {
            _logger.LogInformation("Database tibia initializer - started");
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database tibia initializer - finished");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the database");
            Log.CloseAndFlush();
            throw;
        }
    }
}
