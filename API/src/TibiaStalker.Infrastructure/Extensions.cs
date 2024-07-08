using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TibiaStalker.Infrastructure.Configuration;

namespace TibiaStalker.Infrastructure
{
    public static class Extensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTibiaDbContext(configuration);

            services.AddTibiaOptions(configuration);

            services.AddTibiaHttpClient(configuration);

            return services;
        }
    }
}
