using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.EntityFrameworkCore;
using Shared.Database.Queries.Sql;
using TibiaStalker.Infrastructure.Persistence;

namespace Seeders.Benchmark;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SimpleTestsBenchmark
{
    private List<string> _firstScanNames = new() { "a", "b", "c", "d", "e", "f", "g", "h", "i" };
    private List<string> _secondScanNames = new() { "a", "b", "c", "d", "e", "j", "k", "l", "m" };

    [Benchmark(Baseline = true)]
    public void TestOld()
    {
        var firstSet = new HashSet<string>(_firstScanNames);
        var secondSet = new HashSet<string>(_secondScanNames);

        var loginNames = secondSet.Except(firstSet).ToArray();
        var logoutNames = firstSet.Except(secondSet).ToArray();
    }

    [Benchmark]
    public void TestNew()
    {
        var loginNames = _secondScanNames.Except(_firstScanNames).ToArray();
        var logoutNames = _firstScanNames.Except(_secondScanNames).ToArray();
    }
}
    
