using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Shared.Database.Queries.Sql;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class RemoveCharacterCorrelationsBenchmark
{
    private const string ConnectionString = "Server=localhost;Port=5432;Database=local_database;User Id=sa;Password=pass;";

    private readonly TibiaStalkerDbContext _dbContext = new (new DbContextOptionsBuilder<TibiaStalkerDbContext>()
            .UseNpgsql(ConnectionString).UseSnakeCaseNamingConvention().Options);
    
    [Benchmark(Baseline = true)]
    public async Task RemoveCharacterCorrelationsSqlTogether()
    {
        await _dbContext.ExecuteRawSqlAsync(GenerateQueries.RemoveImpossibleCorrelations);
   }
}
    
