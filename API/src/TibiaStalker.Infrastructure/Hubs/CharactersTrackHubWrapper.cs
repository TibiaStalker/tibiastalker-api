using Microsoft.AspNetCore.SignalR;
using TibiaStalker.Application.SignalR;

namespace TibiaStalker.Infrastructure.Hubs;

public sealed class CharactersTrackHubWrapper : ICharactersTrackHubWrapper
{
    private readonly IHubContext<CharactersTrackHub> _hubContext;

    public CharactersTrackHubWrapper(IHubContext<CharactersTrackHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Internal method for publish messages, using only for internal services
    /// </summary>
    /// <param name="groupName">Name of group with tracking specific character</param>
    /// <param name="data">Data for publish</param>
    public async Task PublishToGroupAsync(string groupName, object data)
    {
        await _hubContext.Clients.Group(groupName).SendAsync("Character Tracker", data);
    }
}