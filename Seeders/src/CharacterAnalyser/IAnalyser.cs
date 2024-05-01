using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;

namespace CharacterAnalyser;

public interface IAnalyser : ISeeder<List<WorldScan>>
{
    Task<List<short>> GetDistinctWorldIdsFromRemainingScans();
    Task<List<WorldScan>> GetWorldScansToAnalyseAsync(short worldId);
}