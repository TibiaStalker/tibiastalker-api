using System.Diagnostics;
using System.Text;
using ChangeNameDetector.Validators;
using ChangeNameDetectorSubscriber.Dtos;
using ChangeNameDetectorSubscriber.Handlers;
using ChangeNameDetectorSubscriber.Subscribers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Shared.Database.Queries.Sql;
using Shared.RabbitMQ.Conventions;
using Shared.RabbitMQ.Events;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace ChangeNameDetectorSubscriber.Events;

public class ChangeNameDetectorEventSubscriber : IEventSubscriber
{
    private readonly ILogger<ChangeNameDetectorEventSubscriber> _logger;
    private readonly IEventResultHandler _eventResultHandler;
    private readonly IRabbitMqConventionProvider _conventionProvider;
    private readonly ITibiaDataClient _tibiaDataClient;
    private readonly INameDetectorValidator _validator;
    private readonly ITibiaStalkerDbContext _dbContext;

    public ChangeNameDetectorEventSubscriber(
        ILogger<ChangeNameDetectorEventSubscriber> logger,
        IEventResultHandler eventResultHandler,
        IRabbitMqConventionProvider conventionProvider,
        ITibiaStalkerDbContext dbContext,
        ITibiaDataClient tibiaDataClient,
        INameDetectorValidator validator)
    {
        _logger = logger;
        _eventResultHandler = eventResultHandler;
        _conventionProvider = conventionProvider;
        _dbContext = dbContext;
        _tibiaDataClient = tibiaDataClient;
        _validator = validator;
    }

    public string GetQueueName()
    {
        var queueBinding = _conventionProvider.GetForType<ChangeNameDetectorEvent>();
        return queueBinding.Queue;
    }

    public async Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var payload = Encoding.UTF8.GetString(ea.Body.Span);
        var eventObject = JsonConvert.DeserializeObject<ChangeNameDetectorEvent>(payload);
        _logger.LogInformation("Event {Event} subscribed. Payload: {Payload}", eventObject.GetType().Name, payload);

        var oldCharacter = await GetCharacterByName(eventObject.CharacterName, cancellationToken);

        if (oldCharacter is null)
        {
            _logger.LogInformation("During event {Event} - cannot find character {characterName}. Payload: {Payload}", eventObject.GetType().Name, eventObject.CharacterName, payload);
            return;
        }

        var isCommitedProperly = await ExecuteInTransactionAsync(Action, payload);

        _eventResultHandler.HandleTransactionResult(isCommitedProperly, nameof(ChangeNameDetectorEvent), payload, eventObject.CharacterName);
        _logger.LogInformation("Event {Event} execution time: {time} ms", eventObject.GetType().Name, stopwatch.ElapsedMilliseconds);

        return;


        async Task Action()
        {
            var fetchedCharacter = await _tibiaDataClient.FetchCharacter(oldCharacter.Name);
            if (fetchedCharacter is null)
            {
                _logger.LogInformation("Method '{methodName}' returned 'null'.", nameof(_tibiaDataClient.FetchCharacter));
            }

            // If Character was not Traded and Character Name is still in database just Update Verified Date.
            else if (!_validator.IsCharacterChangedName(fetchedCharacter, oldCharacter.Name) && !_validator.IsCharacterTraded(fetchedCharacter))
            {
                await RestartDeleteApproachNumber(oldCharacter, cancellationToken);
            }

            // If TibiaData cannot find character just delete with all correlations.
            else if (!_validator.IsCharacterExist(fetchedCharacter))
            {
                await DeleteCharacterWithCorrelations(oldCharacter, cancellationToken);
            }

            // If Character was Traded just delete all correlations.
            else if (_validator.IsCharacterTraded(fetchedCharacter))
            {
                await DeleteCorrelations(oldCharacter, cancellationToken);
            }

            // If name from database was found in former names than merge proper correlations.
            else
            {
                var fetchedCharacterName = fetchedCharacter.Name.ToLower();
                var newCharacter = await GetCharacterByName(fetchedCharacterName, cancellationToken);
                if (newCharacter is null)
                {
                    // If new character name is not yet in the database just change old name to new one.
                    await UpdateCharacterNameAsync(oldCharacter.Name, fetchedCharacterName);
                }
                else
                {
                    await MergeTwoCharacters(oldCharacter, newCharacter, cancellationToken);
                }
            }
        }
    }

    private async Task RestartDeleteApproachNumber(Character character, CancellationToken cancellationToken)
    {
        await _dbContext.Characters.Where(c => c.Name == character.Name)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.DeleteApproachNumber, 0), cancellationToken);
        _logger.LogInformation("Character '{characterName}' was not traded, was not changed name.", character.Name);
    }

    private async Task<Character> GetCharacterByName(string characterName, CancellationToken cancellationToken)
    {
        return await _dbContext.Characters
            .Where(c => c.Name == characterName)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task DeleteCharacterWithCorrelations(Character character, CancellationToken cancellationToken)
    {
        if (character.DeleteApproachNumber >= 3)
        {
            await _dbContext.Characters.Where(c => c.CharacterId == character.CharacterId).ExecuteDeleteAsync(cancellationToken);
            _logger.LogInformation("Character '{characterName}' deleted with correlations.", character.Name);
        }
        else
        {
            await _dbContext.Characters.Where(c => c.Name == character.Name)
                .ExecuteUpdateAsync(update => update.SetProperty(c => c.DeleteApproachNumber, character.DeleteApproachNumber + 1), cancellationToken);
            _logger.LogInformation("Character '{characterName}' deleted approach :{approach}.", character.Name, character.DeleteApproachNumber);
        }
    }

    private async Task DeleteCorrelations(Character character, CancellationToken cancellationToken)
    {
        await _dbContext.CharacterCorrelations.Where(c => c.LoginCharacterId == character.CharacterId || c.LogoutCharacterId == character.CharacterId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.Characters.Where(c => c.CharacterId == character.CharacterId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.TradedDate, DateOnly.FromDateTime(DateTime.Now))
                .SetProperty(c => c.DeleteApproachNumber, 0),
                cancellationToken);

        _logger.LogInformation("Character Correlations of character '{characterName}' deleted.", character.Name);
    }

    private async Task MergeTwoCharacters(Character oldCharacter, Character newCharacter, CancellationToken cancellationToken)
    {
        await ReplaceCharacterIdInCorrelationsAsync(oldCharacter, newCharacter);
        List<CharacterCorrelation> correlations = new();
        List<string> combinedCharacterCorrelations = new();
        List<long> correlationIdsToDelete = new();

        var sameCharacterCorrelations = _dbContext.Database.SqlQueryRaw<string>(GenerateQueries.GetSameCharacterCorrelations, newCharacter.CharacterId).AsEnumerable();

        var sameCharacterCorrelationsCrossed = _dbContext.Database.SqlQueryRaw<string>(GenerateQueries.GetSameCharacterCorrelationsCrossed, newCharacter.CharacterId).AsEnumerable();

        combinedCharacterCorrelations.AddRange(sameCharacterCorrelations);
        combinedCharacterCorrelations.AddRange(sameCharacterCorrelationsCrossed);

        foreach (var row in combinedCharacterCorrelations)
        {
            var combinedCorrelation = JsonConvert.DeserializeObject<CombinedCharacterCorrelation>(row);
            correlationIdsToDelete.Add(combinedCorrelation.FirstCombinedCorrelation.CorrelationId);
            correlationIdsToDelete.Add(combinedCorrelation.SecondCombinedCorrelation.CorrelationId);
            var characterCorrelation = PrepareCharacterCorrelation(combinedCorrelation);
            correlations.Add(characterCorrelation);
        }


        _dbContext.CharacterCorrelations.AddRange(correlations);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Delete already merged CharacterCorrelations
        await _dbContext.CharacterCorrelations.Where(c => correlationIdsToDelete.Contains(c.CorrelationId)).ExecuteDeleteAsync(cancellationToken);
        // Delete old Character
        await _dbContext.Characters.Where(c => c.CharacterId == oldCharacter.CharacterId).ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Characters '{oldCharacterName}/{newCharacterName}' were merged.", oldCharacter.Name, newCharacter.Name);
    }

    private CharacterCorrelation PrepareCharacterCorrelation(CombinedCharacterCorrelation combinedCorrelation)
    {
        return new CharacterCorrelation()
        {
            LoginCharacterId = combinedCorrelation.FirstCombinedCorrelation.LoginCharacterId,
            LogoutCharacterId = combinedCorrelation.FirstCombinedCorrelation.LogoutCharacterId,
            NumberOfMatches =
                (short)(combinedCorrelation.FirstCombinedCorrelation.NumberOfMatches +
                        combinedCorrelation.SecondCombinedCorrelation.NumberOfMatches),
            CreateDate =
                combinedCorrelation.FirstCombinedCorrelation.CreateDate <
                combinedCorrelation.SecondCombinedCorrelation.CreateDate
                    ? combinedCorrelation.FirstCombinedCorrelation.CreateDate
                    : combinedCorrelation.SecondCombinedCorrelation.CreateDate,
            LastMatchDate =
                combinedCorrelation.FirstCombinedCorrelation.LastMatchDate >
                combinedCorrelation.SecondCombinedCorrelation.LastMatchDate
                    ? combinedCorrelation.FirstCombinedCorrelation.LastMatchDate
                    : combinedCorrelation.SecondCombinedCorrelation.LastMatchDate
        };
    }

    private async Task ReplaceCharacterIdInCorrelationsAsync(Character oldCharacter, Character newCharacter)
    {
        await _dbContext.CharacterCorrelations
            .Where(cc => cc.LoginCharacterId == oldCharacter.CharacterId)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.LoginCharacterId, newCharacter.CharacterId));
        await _dbContext.CharacterCorrelations
            .Where(cc => cc.LogoutCharacterId == oldCharacter.CharacterId)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.LogoutCharacterId, newCharacter.CharacterId));
    }

    private async Task<bool> ExecuteInTransactionAsync(Func<Task> action, string payload)
    {
        for (int retryCount = 1; retryCount <= 3; retryCount++)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                await action.Invoke();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError("Method {method} during {action} failed, attempt {retryCount}. Payload {payload}. Error message: {ErrorMessage}",
                    nameof(ExecuteInTransactionAsync), action.Target?.GetType().ReflectedType?.Name, retryCount, payload, ex.Message);
            }
        }

        return false;
    }

    private async Task UpdateCharacterNameAsync(string oldName, string newName)
    {
        await _dbContext.Characters
            .Where(c => c.Name == oldName.ToLower())
            .ExecuteUpdateAsync(update => update
                .SetProperty(c => c.Name, newName.ToLower()));

        _logger.LogInformation("Character name '{character}' updated to '{newCharacter}'", oldName, newName);
    }
}