using Microsoft.EntityFrameworkCore;
using TibiaStalker.Application.Exceptions;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace TibiaStalker.Infrastructure.Services;

public class TrackCharacterService : ITrackCharacterService
{
    private readonly ITibiaDataClient _tibiaDataClient;
    private readonly ITibiaStalkerDbContext _dbContext;

    public TrackCharacterService(ITibiaDataClient tibiaDataClient, ITibiaStalkerDbContext dbContext)
    {
        _tibiaDataClient = tibiaDataClient;
        _dbContext = dbContext;
    }

    public async Task CreateTrack(string characterName, string connectionId)
    {
        var fetchedCharacter = await _tibiaDataClient.FetchCharacterWithRetry(characterName.Trim());

        if (fetchedCharacter is null)
        {
            throw new TibiaDataApiConnectionException();
        }
        if (string.IsNullOrWhiteSpace(fetchedCharacter.Name))
        {
            throw new NotFoundException(nameof(Character), characterName);
        }

        var existingEntity = _dbContext.TrackedCharacters.FirstOrDefault(t => t.Name == characterName && t.ConnectionId == connectionId);
        if (existingEntity is not null)
        {
            throw new EntityAlreadyExistsException(characterName);
        }

        var entity = new TrackedCharacter(characterName, fetchedCharacter.World, connectionId);
        _dbContext.TrackedCharacters.Add(entity);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveTrack(string characterName, string connectionId)
    {
        await _dbContext.TrackedCharacters.Where(tc => tc.Name == characterName && tc.ConnectionId == connectionId)
            .ExecuteDeleteAsync();
    }

    public async Task RemoveTracksByConnectionId(string connectionId)
    {
        await _dbContext.TrackedCharacters.Where(tc => tc.ConnectionId == connectionId)
            .ExecuteDeleteAsync();
    }
}