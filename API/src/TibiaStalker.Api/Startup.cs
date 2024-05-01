using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Autofac;
using MediatR;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using TibiaStalker.Api.Configurations;
using TibiaStalker.Api.Filters;
using TibiaStalker.Api.Swagger;
using TibiaStalker.Infrastructure;
using TibiaStalker.Infrastructure.Configuration;
using TibiaStalker.Infrastructure.HealthChecks;
using TibiaStalker.Infrastructure.Middlewares;

namespace TibiaStalker.Api;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        TibiaStalker.Infrastructure.Configuration.LoggerConfiguration.ConfigureLogger(_configuration,
            Assembly.GetExecutingAssembly().GetName().Name);
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<AutofacModule>();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddInfrastructure(_configuration);

        services.AddControllers(opt => { opt.Filters.Add<ErrorHandlingFilterAttribute>(); })
            .AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });

        services.AddRateLimiter(StartupConfigurations.RateLimitedConfiguration);
        services.AddRouting(options => { options.LowercaseUrls = true; });
        services.AddMediatR(typeof(Startup));
        services.AddSignalR();
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.Cookie.Name = "sessionId";
            options.Cookie.HttpOnly = true;
            options.IdleTimeout = TimeSpan.FromMinutes(20);
            options.Cookie.IsEssential = true;
        });

        services.AddCors(options =>
        {
            options.AddPolicy("TibiaStalkerApi", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1.0);
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddTransient<GlobalExceptionHandlingMiddleware>();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(options =>
        {
            // Add a custom operation filter which sets default values
            options.OperationFilter<SwaggerDefaultValues>();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        StartupConfigurations.ConfigureOptions(services);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(StartupConfigurations.SwaggerUiConfiguration(app));

        app.UseTibiaHealthChecks();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseSession();
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.None,
            Secure = CookieSecurePolicy.None
        });
        app.UseMiddleware<RequestLoggerMiddleware>();
        app.UseRateLimiter();
        app.UseSerilogRequestLogging(StartupConfigurations.SerilogRequestLoggingConfiguration);
        app.UseCors("TibiaStalkerApi");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.UseSignalrHubs();
    }
}