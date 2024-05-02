using System.Diagnostics;
using ChangeNameDetector.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace ChangeNameDetector.Services;

public class ChangeNameDetectorService : IChangeNameDetectorService
{
    private readonly ILogger<ChangeNameDetectorService> _logger;
    private readonly INameDetectorValidator _validator;
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly ITibiaDataClient _tibiaDataClient;
    private readonly IEventPublisher _publisher;

    public ChangeNameDetectorService(ILogger<ChangeNameDetectorService> logger,
        INameDetectorValidator validator,
        ITibiaStalkerDbContext dbContext,
        ITibiaDataClient tibiaDataClient,
        IEventPublisher publisher)
    {
        _logger = logger;
        _validator = validator;
        _dbContext = dbContext;
        _tibiaDataClient = tibiaDataClient;
        _publisher = publisher;
    }

    public async Task Run()
    {
        while (true)
        {
            var stopwatch = Stopwatch.StartNew();
            var stopwatch2 = Stopwatch.StartNew();

            var character = await GetFirstCharacterByVerifiedDateAsync();

            if (character is null)
            {
                break;
            }

            stopwatch2.Stop();
            _logger.LogInformation("Get character '{characterName}' from DB. Execution time : {time} ms",
                character.Name, stopwatch2.ElapsedMilliseconds);

            var stopwatch3 = Stopwatch.StartNew();
            var fetchedCharacter = await _tibiaDataClient.FetchCharacter(character.Name);
            if (fetchedCharacter is null)
            {
                continue;
            }

            stopwatch3.Stop();
            _logger.LogInformation("Fetch character '{characterName}' from API. Execution time : {time} ms",
                character.Name, stopwatch3.ElapsedMilliseconds);

            // If Character was not Traded and Character Name is still in database just Update Verified Date.
            if (!_validator.IsCharacterChangedName(fetchedCharacter, character) && !_validator.IsCharacterTraded(fetchedCharacter))
            {
                stopwatch.Stop();
                _logger.LogInformation("Character '{characterName}' was not traded, was not changed name. Checked in execution time : {time} ms",
                    character.Name, stopwatch.ElapsedMilliseconds);
            }


            // If TibiaData cannot find character just delete with all correlations.
            else if (!_validator.IsCharacterExist(fetchedCharacter))
            {
                await _publisher.PublishAsync($"'{character.Name}' ({DateTime.Now})",
                    new DeleteCharacterWithCorrelationsEvent(character.CharacterId));
            }


            // If Character was Traded just delete all correlations.
            else if (_validator.IsCharacterTraded(fetchedCharacter))
            {
                await _publisher.PublishAsync($"'{character.Name}' ({DateTime.Now})",
                    new DeleteCharacterCorrelationsEvent(character.CharacterId));
            }


            // If name from database was found in former names than merge proper correlations.
            else
            {
                var fetchedCharacterName = fetchedCharacter.Name;

                var newCharacter = await _dbContext.Characters.Where(c => c.Name == fetchedCharacterName.ToLower()).FirstOrDefaultAsync();

                if (newCharacter is null)
                {
                    // If new character name is not yet in the database just change old name to new one.
                    await UpdateCharacterNameAsync(character.Name, fetchedCharacterName);
                    _logger.LogInformation("Character name '{character}' updated to '{newCharacter}'", character.Name, fetchedCharacterName.ToLower());
                }
                else
                {
                    await _publisher.PublishAsync($"'{character.Name}' / '{newCharacter.Name}' ({DateTime.Now})",
                        new MergeTwoCharactersEvent(character.CharacterId, newCharacter.CharacterId));
                }
            }

            await UpdateCharacterVerifiedDate(character.CharacterId);
            _dbContext.ChangeTracker.Clear();
        }
    }

    private async Task<Character> GetFirstCharacterByVerifiedDateAsync()
    {
        var visibilityOfTradeProperty = DateOnly.FromDateTime(DateTime.Now.AddDays(-31));
        var scanPeriod = DateOnly.FromDateTime(DateTime.Now.AddDays(-20));

        return await _dbContext.Characters
            .Where(c => (!c.TradedDate.HasValue || c.TradedDate < visibilityOfTradeProperty)
                        && (!c.VerifiedDate.HasValue || c.VerifiedDate < scanPeriod))
            .OrderByDescending(c => c.VerifiedDate == null)
            .ThenBy(c => c.VerifiedDate)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    private async Task UpdateCharacterNameAsync(string oldName, string newName)
    {
        await _dbContext.Characters
            .Where(c => c.Name == oldName.ToLower())
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.Name, newName.ToLower()));
    }

    private async Task UpdateCharacterVerifiedDate(int characterId)
    {
        await _dbContext.Characters
            .Where(c => c.CharacterId == characterId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.VerifiedDate, DateOnly.FromDateTime(DateTime.Now)));
    }
}