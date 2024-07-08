using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

namespace TibiaStalker.Infrastructure.Middlewares;

public static class HealthCheckMiddleware
{
    public static void UseTibiaHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions()
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            exception = e.Value.Exception?.Message,
                            duration = e.Value.Duration.ToString()
                        })
                    }));
            }
        });
    }
}