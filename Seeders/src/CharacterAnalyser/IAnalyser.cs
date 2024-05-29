using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;

namespace CharacterAnalyser;

public interface IAnalyser : ISeeder<List<WorldScan>>
{
    List<short> GetDistinctWorldIdsFromRemainingScans();
    List<WorldScan> GetWorldScansToAnalyse(short worldId);
}