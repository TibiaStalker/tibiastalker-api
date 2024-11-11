using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using TibiaStalker.Infrastructure.Persistence;

// ReSharper disable RedundantAnonymousTypePropertyName

namespace TibiaStalker.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CreateCharacterCorrelationsIfNotExistBenchmark
{
    private const string ConnectionString = "Server=localhost;Port=5432;Database=local_database;User Id=sa;Password=pass;";

    private readonly TibiaStalkerDbContext _dbContext = new (new DbContextOptionsBuilder<TibiaStalkerDbContext>()
            .UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention().Options);
    
    [Benchmark(Baseline = true)]
    public Task CreateCharacterCorrelationsIfNotExistEfCoreOld()
    {
        return Task.CompletedTask;
    }
}
    
