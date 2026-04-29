using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace TibiaStalker.Infrastructure.Policies;

public class GlobalRateLimiterPolicy : AbstractRateLimiterPolicy, IRateLimiterPolicy<string>
{
    public GlobalRateLimiterPolicy() : base()
    {
    }

    public override RateLimitPartition<string> GetPartition(HttpContext httpContext)
    {
        var partitionKey = httpContext.User.Identity?.Name 
                           ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                           ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 3,
                Window = TimeSpan.FromSeconds(5),
            });
    }
}