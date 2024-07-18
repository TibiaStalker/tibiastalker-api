using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyser;

public interface IAnalyser
{
    List<short> GetDistinctWorldIdsFromRemainingScans();
    List<WorldScan> GetTwoWorldScansToAnalyse(short worldId);
    Task SoftDeleteWorldScanAsync(int scanId);
}