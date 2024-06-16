using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyser;

public interface IAnalyser
{
    List<short> GetDistinctWorldIdsFromRemainingScans();
    List<WorldScan> GetWorldScansToAnalyse(short worldId);
    Task SoftDeleteWorldScanAsync(int scanId);
}