using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TibiaStalker.Application.Services;

namespace TibiaStalker.Infrastructure.Hubs;

public class CharactersTrackHub : Hub
{
    private readonly ILogger<CharactersTrackHub> _logger;
    private readonly ITrackCharacterService _trackCharacterService;

    public CharactersTrackHub(ILogger<CharactersTrackHub> logger, ITrackCharacterService trackCharacterService)
    {
        _logger = logger;
        _trackCharacterService = trackCharacterService;
    }

    public override async Task OnConnectedAsync()
    {
        var realUserIp = GetRealUserIp();

        _logger.LogInformation("Client connected - UserId: {UserId}. ConnectionId: {ConnectionId}. ", realUserIp, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await _trackCharacterService.RemoveTracksByConnectionId(Context.ConnectionId);

        var realUserIp = GetRealUserIp();

        _logger.LogInformation("Client disconnected - UserId: {UserId}. ConnectionId: {ConnectionId}. Exception: {Exception}",
            realUserIp, Context.ConnectionId, exception?.ToString());

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// External method for external services e.g. Web Application
    /// </summary>
    /// <param name="groupName">Name of group to track specific character</param>
    public async Task JoinGroup(string groupName)
    {
        groupName = groupName.ToLower();

        await _trackCharacterService.CreateTrack(groupName, Context.ConnectionId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        var realUserIp = GetRealUserIp();

        _logger.LogInformation("UserId: {UserId} - joined group: {groupName}. ConnectionId: {ConnectionId}.",
            realUserIp, groupName, Context.ConnectionId);
    }

    /// <summary>
    /// External method for external services e.g. Web Application
    /// </summary>
    /// <param name="groupName">Name of group to track specific character</param>
    public async Task LeaveGroup(string groupName)
    {
        groupName = groupName.ToLower();

        await _trackCharacterService.RemoveTrack(groupName, Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        var realUserIp = GetRealUserIp();

        _logger.LogInformation("UserId: {UserId} - leaved group: {groupName}. ConnectionId: {ConnectionId}.",
            realUserIp, groupName, Context.ConnectionId);
    }

    private string GetRealUserIp()
    {
        var feature = Context.Features.Get<IHttpConnectionFeature>();
        var remoteAddress = feature.RemoteIpAddress?.ToString();
        var remoteAddressFromHeader = Context.GetHttpContext()?.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        var realIp = remoteAddressFromHeader ?? remoteAddress;
        return realIp;
    }
}