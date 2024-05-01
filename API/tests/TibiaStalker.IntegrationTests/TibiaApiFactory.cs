﻿using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using TibiaStalker.Api;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.IntegrationTests;

public class TibiaApiFactory : WebApplicationFactory<Startup>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer =
        new PostgreSqlBuilder()
            .WithImage("postgres:14-alpine")
            .WithDatabase("postgres")
            .WithUsername("username")
            .WithPassword("password")
            .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();
        await dbContext.Database.MigrateAsync();
        
        await dbContext.Worlds.AddRangeAsync(CreateWorlds());
        await dbContext.Characters.AddRangeAsync(CreateCharacters());
        await dbContext.CharacterCorrelations.AddRangeAsync(CreateCharacterCorrelaions());
        
        await dbContext.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TibiaStalkerDbContext>));
            if (descriptor != null) 
                services.Remove(descriptor);
            
            services.AddSingleton(Options.Create(new ConnectionStringsSection { PostgreSql = _dbContainer.GetConnectionString() }));
            services.AddDbContext<TibiaStalkerDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()).UseSnakeCaseNamingConvention());
        });
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
    
    private IEnumerable<World> CreateWorlds()
    {
        return new List<World>()
        {
            new() { WorldId = 111, Name = "Premia", Url = "urlPremia", IsAvailable = true },
            new() { WorldId = 112, Name = "Vunira", Url = "urlVunira", IsAvailable = true },
            new() { WorldId = 113, Name = "Antica", Url = "urlAntica", IsAvailable = false }
        };
    }
    
    private IEnumerable<Character> CreateCharacters()
    {
        return new List<Character>()
        {
            new() { CharacterId = 111, Name = "abargo maewa", WorldId = 111 },
            new() { CharacterId = 112, Name = "amy winehousse", WorldId = 111 },
            new() { CharacterId = 113, Name = "duzzerah", WorldId = 111 },
    
            new() { CharacterId = 114, Name = "ziomal rafiego", WorldId = 112},
            new() { CharacterId = 115, Name = "zanteey", WorldId = 112 },
            new() { CharacterId = 116, Name = "artenian", WorldId = 112 },
        };
    }
    
    private IEnumerable<CharacterCorrelation> CreateCharacterCorrelaions()
    {
        return new List<CharacterCorrelation>()
        {
            new() { LoginCharacterId = 111, LogoutCharacterId = 112, NumberOfMatches = 8 },
            new() { LoginCharacterId = 113, LogoutCharacterId = 111, NumberOfMatches = 4 },
            new() { LoginCharacterId = 113, LogoutCharacterId = 112, NumberOfMatches = 2 },
    
            new() { LoginCharacterId = 114, LogoutCharacterId = 116, NumberOfMatches = 10 },
            new() { LoginCharacterId = 116, LogoutCharacterId = 115, NumberOfMatches = 21 },
            new() { LoginCharacterId = 115, LogoutCharacterId = 116, NumberOfMatches = 1 },
        };
    }
}