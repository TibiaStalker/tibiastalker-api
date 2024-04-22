namespace TibiaStalker.Application.SignalR;

public interface ICharactersTrackHubWrapper
{
    Task PublishToGroupAsync(string groupName, object data);
}