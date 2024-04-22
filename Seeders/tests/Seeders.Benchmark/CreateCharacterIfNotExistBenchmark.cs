using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Shared.Database.Queries.Sql;
using TibiaStalker.Infrastructure.Persistence;

namespace Seeders.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class CreateCharacterIfNotExistBenchmark
{
    private const string _connectionString = "Server=localhost;Port=5432;Database=local_database;User Id=sa;Password=pass;";

    private readonly TibiaStalkerDbContext _dbContext = new (new DbContextOptionsBuilder<TibiaStalkerDbContext>()
            .UseNpgsql(_connectionString).UseSnakeCaseNamingConvention().Options);
    
    [Benchmark(Baseline = true)]
    public async Task CreateCharacterIfNotExistSql()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(GenerateQueries.CreateCharactersIfNotExists);
    }

    private IQueryable<string> CharactersNames()
    {
        return _dbContext.CharacterActions
            .Select(ca => ca.CharacterName)
            .Distinct();
    }

}
    
