using Microsoft.Extensions.Diagnostics.HealthChecks;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ITibiaStalkerDbContext _dbContext;

    public DatabaseHealthCheck(ITibiaStalkerDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new ())
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}