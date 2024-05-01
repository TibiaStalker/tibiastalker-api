﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace Seeders.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CreateCharacterCorrelationsIfNotExistBenchmark
{
    private const string _connectionString = "Server=localhost;Port=5432;Database=local_database;User Id=sa;Password=pass;";

    private readonly TibiaStalkerDbContext _dbContext = new (new DbContextOptionsBuilder<TibiaStalkerDbContext>()
            .UseNpgsql(_connectionString).UseSnakeCaseNamingConvention().Options);
    
    [Benchmark(Baseline = true)]
    public async Task CreateCharacterCorrelationsIfNotExistEfCoreOld()
    {
        var lastMatchDate = (await _dbContext.CharacterActions.FirstAsync()).LogoutOrLoginDate;
        var loginCharactersIds = CharactersIds(true);
        var logoutCharactersIds = CharactersIds(false);
        
        var correlationsDataToCreate = loginCharactersIds
            .SelectMany(login => logoutCharactersIds,
                (login, logout) => new { LoginCharacterId = login, LogoutCharacterId = logout });
        
        var existingCharacterCorrelationsPart1 =
            _dbContext.Characters
                .Where(c => loginCharactersIds.Contains(c.CharacterId))
                .SelectMany(c => c.LoginCharacterCorrelations.Select(cc => new {LoginCharacterId = cc.LoginCharacterId, LogoutCharacterId = cc.LogoutCharacterId}));
        
        var existingCharacterCorrelationsPart2 =
            _dbContext.Characters
                .Where(c => logoutCharactersIds.Contains(c.CharacterId))
                .SelectMany(c => c.LoginCharacterCorrelations.Select(cc => new {LoginCharacterId = cc.LogoutCharacterId, LogoutCharacterId = cc.LoginCharacterId}));
  
        var correlationsDataToInsert = correlationsDataToCreate.Except(existingCharacterCorrelationsPart1).Except(existingCharacterCorrelationsPart2);
     
        var newCorrelations = correlationsDataToInsert
            .Select(cc => new CharacterCorrelation
            {
                LoginCharacterId = cc.LoginCharacterId,
                LogoutCharacterId = cc.LogoutCharacterId,
                NumberOfMatches = 1,
                CreateDate = lastMatchDate,
                LastMatchDate = lastMatchDate
            }).ToList();

        _dbContext.CharacterCorrelations.AddRange(newCorrelations);
        await _dbContext.SaveChangesAsync();
    }
    
    [Benchmark]
    public async Task CreateCharacterCorrelationsIfNotExistEfCoreNew()
    {
        var lastMatchDate = (await _dbContext.CharacterActions.FirstAsync()).LogoutOrLoginDate;
        var loginCharactersIds = CharactersIds(true);
        var logoutCharactersIds = CharactersIds(false);
        
        var correlationsDataToCreate = loginCharactersIds
            .SelectMany(login => logoutCharactersIds,
                (login, logout) => new { LoginCharacterId = login, LogoutCharacterId = logout });
        
        var existingCharacterCorrelationsPart1 =
            _dbContext.Characters
                .Where(c => loginCharactersIds.Contains(c.CharacterId))
                .SelectMany(c => c.LoginCharacterCorrelations.Select(cc => new {LoginCharacterId = cc.LoginCharacterId, LogoutCharacterId = cc.LogoutCharacterId}));
        
        var existingCharacterCorrelationsPart2 =
            _dbContext.Characters
                .Where(c => logoutCharactersIds.Contains(c.CharacterId))
                .SelectMany(c => c.LoginCharacterCorrelations.Select(cc => new {LoginCharacterId = cc.LogoutCharacterId, LogoutCharacterId = cc.LoginCharacterId}));
  
        var newCorrelations = correlationsDataToCreate
            .Where(cc => !existingCharacterCorrelationsPart1.Concat(existingCharacterCorrelationsPart2)
                .Any(ec => ec.LoginCharacterId == cc.LoginCharacterId && ec.LogoutCharacterId == cc.LogoutCharacterId))
            .Select(cc => new CharacterCorrelation
            {
                LoginCharacterId = cc.LoginCharacterId,
                LogoutCharacterId = cc.LogoutCharacterId,
                NumberOfMatches = 1,
                CreateDate = lastMatchDate,
                LastMatchDate = lastMatchDate
            }).ToList();
        
         _dbContext.CharacterCorrelations.AddRange(newCorrelations);
        await _dbContext.SaveChangesAsync();
    }
    
    private IQueryable<int> CharactersIds(bool isOnline)
    {
        return _dbContext.Characters
            .Where(c =>
                _dbContext.CharacterActions
                    .Where(ca => ca.IsOnline == isOnline)
                    .Select(ca => ca.CharacterName)
                    .Distinct().Contains(c.Name))
            .Select(ca => ca.CharacterId);
    }


}
    
