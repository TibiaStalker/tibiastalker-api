using TibiaStalker.Application.TibiaData.Dtos;

namespace TibiaStalker.Application.Interfaces;

public interface ITibiaDataClient
{
    Task<IReadOnlyList<string>> FetchWorldsNames();
    Task<IReadOnlyList<string>> FetchCharactersOnline(string worldName);
    Task<CharacterResult> FetchCharacterWithRetry(string characterName);
    Task<CharacterResult> FetchCharacterWithoutRetry(string characterName);
}