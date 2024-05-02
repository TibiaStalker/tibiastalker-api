using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Infrastructure.Configuration;
using TibiaStalker.Infrastructure.Policies;
using TibiaStalker.Infrastructure.Traceability;
using TibiaStalker.Infrastructure.Clients.TibiaData;
using TibiaStalker.Infrastructure.Hubs;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TibiaStalkerDbContext>(opt => opt
                .UseNpgsql(
                    configuration.GetConnectionString(nameof(ConnectionStringsSection.PostgreSql)),
                    options =>
                    {
                        options
                            .MinBatchSize(1)
                            .MaxBatchSize(30)
                            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                            .CommandTimeout(configuration
                                .GetSection(
                                    $"{EfCoreConfigurationSection.SectionName}:{EfCoreConfigurationSection.CommandTimeout}")
                                .Get<int>());
                    })
                .UseSnakeCaseNamingConvention());

            services.AddOptions<TibiaDataSection>()
                .Bind(configuration.GetSection(TibiaDataSection.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddTibiaHttpClient(configuration);

            return services;
        }

        public static void UseSignalrHubs(this IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CharactersTrackHub>(HubRoutes.CharactersTrackHub, options =>
                {
                    options.CloseOnAuthenticationExpiration = true;
                });
            });
        }

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
}
