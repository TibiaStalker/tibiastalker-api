using System.Diagnostics;
using ChangeNameDetector.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Infrastructure.Persistence;

namespace ChangeNameDetector.Services;

public class ChangeNameDetectorService : IChangeNameDetectorService
{
    private readonly ILogger<ChangeNameDetectorService> _logger;
    private readonly INameDetectorValidator _validator;
    private readonly ITibiaStalkerDbContext _dbContext;
    private readonly ITibiaDataClient _tibiaDataClient;
    private readonly IEventPublisher _publisher;
    private readonly ChangeNameDetectorSection _changeNameDetectorOptions;

    public ChangeNameDetectorService(ILogger<ChangeNameDetectorService> logger,
        INameDetectorValidator validator,
        ITibiaStalkerDbContext dbContext,
        ITibiaDataClient tibiaDataClient,
        IEventPublisher publisher,
        IOptions<SeederVariablesSection> options)
    {
        _logger = logger;
        _validator = validator;
        _dbContext = dbContext;
        _tibiaDataClient = tibiaDataClient;
        _publisher = publisher;
        _changeNameDetectorOptions = options.Value.ChangeNameDetector;
    }

    public async Task Run()
    {
        while (true)
        {
            var stopwatch = Stopwatch.StartNew();
            var stopwatch1 = Stopwatch.StartNew();

            var characterNameFromDb = await GetFirstCharacterByVerifiedDateAsync();

            if (characterNameFromDb is null)
            {
                break;
            }

            _logger.LogInformation("Get character '{characterName}' from DB. Execution time : {time} ms",
                characterNameFromDb, stopwatch.ElapsedMilliseconds);

            stopwatch.Restart();
            var fetchedCharacter = await _tibiaDataClient.FetchCharacter(characterNameFromDb);
            if (fetchedCharacter is null)
            {
                continue;
            }

            _logger.LogInformation("Fetch character '{characterName}' from API. Execution time : {time} ms",
                characterNameFromDb, stopwatch.ElapsedMilliseconds);

            // If Character was not Traded and Character Name is still in database just Update Verified Date.
            if (!_validator.IsCharacterChangedName(fetchedCharacter, characterNameFromDb) && !_validator.IsCharacterTraded(fetchedCharacter))
            {
                _logger.LogInformation("Character '{characterName}' was not traded, was not changed name.", characterNameFromDb);
            }


            // If TibiaData cannot find character just delete with all correlations.
            else if (!_validator.IsCharacterExist(fetchedCharacter))
            {
                await _publisher.PublishAsync($"'{characterNameFromDb}' ({DateTime.Now})", new DeleteCharacterWithCorrelationsEvent(characterNameFromDb));
            }


            // If Character was Traded just delete all correlations.
            else if (_validator.IsCharacterTraded(fetchedCharacter))
            {
                await _publisher.PublishAsync($"'{characterNameFromDb}' ({DateTime.Now})", new DeleteCharacterCorrelationsEvent(characterNameFromDb));
            }


            // If name from database was found in former names than merge proper correlations.
            else
            {
                var fetchedCharacterName = fetchedCharacter.Name;

                var newCharacter = await _dbContext.Characters.Where(c => c.Name == fetchedCharacterName.ToLower()).FirstOrDefaultAsync();

                if (newCharacter is null)
                {
                    // If new character name is not yet in the database just change old name to new one.
                    await UpdateCharacterNameAsync(characterNameFromDb, fetchedCharacterName);
                    _logger.LogInformation("Character name '{character}' updated to '{newCharacter}'", characterNameFromDb, fetchedCharacterName.ToLower());
                }
                else
                {
                    await _publisher.PublishAsync($"'{characterNameFromDb}' / '{newCharacter.Name}' ({DateTime.Now})",
                        new MergeTwoCharactersEvent(characterNameFromDb, newCharacter.Name));
                }
            }

            await UpdateCharacterVerifiedDate(characterNameFromDb);
            _dbContext.ChangeTracker.Clear();
            _logger.LogInformation("Character '{characterName}' checked. Execution time : {time} ms", characterNameFromDb, stopwatch1.ElapsedMilliseconds);
        }
    }

    private async Task<string> GetFirstCharacterByVerifiedDateAsync()
    {
        // Sign "-" is for back time
        var visibilityOfTradePeriodDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-_changeNameDetectorOptions.VisibilityOfTradePeriod));
        var scanPeriodDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-_changeNameDetectorOptions.ScanPeriod));

        return await _dbContext.Characters
            .Where(c => c.TradedDate < visibilityOfTradePeriodDate && c.VerifiedDate < scanPeriodDate)
            .OrderBy(c => c.VerifiedDate)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
    }

    private async Task UpdateCharacterNameAsync(string oldName, string newName)
    {
        await _dbContext.Characters
            .Where(c => c.Name == oldName.ToLower())
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.Name, newName.ToLower()));
    }

    private async Task UpdateCharacterVerifiedDate(string characterName)
    {
        await _dbContext.Characters
            .Where(c => c.Name == characterName)
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.VerifiedDate, DateOnly.FromDateTime(DateTime.Now)));
    }
}