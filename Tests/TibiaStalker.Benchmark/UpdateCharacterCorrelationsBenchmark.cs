using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class UpdateCharacterCorrelationsBenchmark
{
    private const string ConnectionString = "Server=localhost;Port=5432;Database=local_database;User Id=sa;Password=pass;";

    private readonly TibiaStalkerDbContext _dbContext = new (new DbContextOptionsBuilder<TibiaStalkerDbContext>()
            .UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention().Options);
    
    [Benchmark(Baseline = true)]
    public Task UpdateCharacterCorrelationsEfCore()
    {
        return Task.CompletedTask;
    }
    
    [Benchmark]
    public Task UpdateCharacterCorrelationsEfCoreNew()
    {
        return Task.CompletedTask;
    }
}
    
