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

    }
}
    
