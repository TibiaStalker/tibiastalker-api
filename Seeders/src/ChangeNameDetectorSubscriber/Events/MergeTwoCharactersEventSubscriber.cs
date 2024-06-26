﻿using System.Text;
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
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace ChangeNameDetectorSubscriber.Events;

public class MergeTwoCharactersEventSubscriber : IEventSubscriber
{
    private readonly ILogger<MergeTwoCharactersEventSubscriber> _logger;
    private readonly IEventResultHandler _eventResultHandler;
    private readonly IRabbitMqConventionProvider _conventionProvider;
    private readonly ITibiaStalkerDbContext _dbContext;

    public MergeTwoCharactersEventSubscriber(
        ILogger<MergeTwoCharactersEventSubscriber> logger,
        IEventResultHandler eventResultHandler,
        IRabbitMqConventionProvider conventionProvider,
        ITibiaStalkerDbContext dbContext)
    {
        _logger = logger;
        _eventResultHandler = eventResultHandler;
        _conventionProvider = conventionProvider;
        _dbContext = dbContext;
    }

    public string GetQueueName()
    {
        var queueBinding = _conventionProvider.GetForType<MergeTwoCharactersEvent>();
        return queueBinding.Queue;
    }

    public async Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        var payload = Encoding.UTF8.GetString(ea.Body.Span);
        var eventObject = JsonConvert.DeserializeObject<MergeTwoCharactersEvent>(payload);
        _logger.LogInformation("Event {Event} subscribed. Payload: {Payload}", eventObject.GetType().Name, payload);

        Thread.Sleep(1000);

        var oldCharacter = await _dbContext.Characters
            .Where(c => c.Name == eventObject.OldCharacterName)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        var newCharacter = await _dbContext.Characters
            .Where(c => c.Name == eventObject.NewCharacterName)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (oldCharacter is null || newCharacter is null)
        {
            _logger.LogInformation(
                "During event {Event} - cannot find character one of the characters in database. Payload: {Payload}",
                eventObject.GetType().Name, payload);
            return;
        }

        var isCommitedProperly = await ExecuteInTransactionAsync(Action, payload);

        _eventResultHandler.HandleTransactionResult(isCommitedProperly, nameof(MergeTwoCharactersEvent), payload, oldCharacter.Name);

        await _dbContext.Characters
            .Where(c => c.CharacterId == oldCharacter.CharacterId)
            .ExecuteDeleteAsync(cancellationToken);
        return;

        async Task Action()
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
            await _dbContext.CharacterCorrelations.Where(c => correlationIdsToDelete.Contains(c.CorrelationId))
                .ExecuteDeleteAsync(cancellationToken);
        }
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

    private async Task ReplaceCharacterIdInCorrelationsAsync(Character oldCharacter, Character newCharacter)
    {
        await _dbContext.CharacterCorrelations
            .Where(cc => cc.LoginCharacterId == oldCharacter.CharacterId)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.LoginCharacterId, newCharacter.CharacterId));
        await _dbContext.CharacterCorrelations
            .Where(cc => cc.LogoutCharacterId == oldCharacter.CharacterId)
            .ExecuteUpdateAsync(update => update.SetProperty(c => c.LogoutCharacterId, newCharacter.CharacterId));
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
}