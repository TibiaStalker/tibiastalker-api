using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyser.ActionRules.Rules;

public class NumberOfWorldScansShouldBe2Rule : IRule
{
    private const int _numberOfElements = 2;
    private readonly List<WorldScan> _worldScans;

    public NumberOfWorldScansShouldBe2Rule(List<WorldScan> worldScans)
    {
        _worldScans = worldScans;
    }

    public bool IsBroken() => _worldScans.Count != _numberOfElements;
}