using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using TibiaStalker.Infrastructure.Hubs;

namespace TibiaStalker.Infrastructure.Middlewares;

public static class EndpointsMiddleware
{
    public static void UseTibiaEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapHub<CharactersTrackHub>(HubRoutes.CharactersTrackHub, options => // SignalR configuration
            {
                options.CloseOnAuthenticationExpiration = true;
            });
        });
    }
}