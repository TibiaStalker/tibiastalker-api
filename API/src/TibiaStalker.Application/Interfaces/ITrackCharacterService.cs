namespace TibiaStalker.Application.Interfaces;

public interface ITrackCharacterService
{
    Task CreateTrack(string characterName, string connectionId);
    Task RemoveTrack(string characterName, string connectionId);
    Task RemoveTracksByConnectionId(string connectionId);
}