using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyserSubscriber.Events.ActionRules.Rules;

public class TimeBetweenWorldScansCannotBeLongerThanMaxDurationRule : IRule
{
    private const int MaxDurationMinutes = 6;
    private readonly WorldScan[] _worldScans;

    public TimeBetweenWorldScansCannotBeLongerThanMaxDurationRule(WorldScan[] worldScans)
    {
        _worldScans = worldScans;
    }
    public bool IsBroken()
    {
        var timeDifference = _worldScans[1].ScanCreateDateTime - _worldScans[0].ScanCreateDateTime;

        return timeDifference.TotalMinutes > MaxDurationMinutes;
    }
}