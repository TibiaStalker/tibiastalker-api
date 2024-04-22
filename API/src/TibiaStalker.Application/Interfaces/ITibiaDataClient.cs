using TibiaStalker.Application.TibiaData.Dtos;

namespace TibiaStalker.Application.Interfaces;

public interface ITibiaDataClient
{
    public Task<IReadOnlyList<string>> FetchWorldsNames();
    public Task<IReadOnlyList<string>> FetchCharactersOnline(string worldName);
    Task<CharacterResult> FetchCharacter(string characterName, bool withRetryPolicy = true);
}