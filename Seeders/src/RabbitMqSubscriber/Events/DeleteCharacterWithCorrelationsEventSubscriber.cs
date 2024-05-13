using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using RabbitMqSubscriber.Handlers;
using RabbitMqSubscriber.Subscribers;
using Shared.RabbitMQ.Conventions;
using Shared.RabbitMQ.Events;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace RabbitMqSubscriber.Events;

public class DeleteCharacterWithCorrelationsEventSubscriber : IEventSubscriber
{
    private readonly ILogger<DeleteCharacterWithCorrelationsEventSubscriber> _logger;
    private readonly IEventResultHandler _eventResultHandler;
    private readonly IRabbitMqConventionProvider _conventionProvider;
    private readonly ITibiaStalkerDbContext _dbContext;

    public DeleteCharacterWithCorrelationsEventSubscriber(
        ILogger<DeleteCharacterWithCorrelationsEventSubscriber> logger,
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
        var queueBinding = _conventionProvider.GetForType<DeleteCharacterWithCorrelationsEvent>();
        return queueBinding.Queue;
    }

    public async Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        var payload = Encoding.UTF8.GetString(ea.Body.Span);
        var eventObject = JsonConvert.DeserializeObject<DeleteCharacterWithCorrelationsEvent>(payload);
        _logger.LogInformation("Event {Event} subscribed. Payload: {Payload}", eventObject.GetType().Name, payload);

        Thread.Sleep(1000);

        var character = new Character();

        var isCommitedProperly = await ExecuteInTransactionAsync(Action, payload);

        _eventResultHandler.HandleTransactionResult(isCommitedProperly, nameof(DeleteCharacterWithCorrelationsEvent), payload, character.Name);
        return;

        async Task Action()
        {
            character = await _dbContext.Characters.Where(c => c.Name == eventObject.CharacterName)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (character.DeleteApproachNumber >= 3)
            {
                await _dbContext.Characters.Where(c => c.CharacterId == character.CharacterId)
                    .ExecuteDeleteAsync(cancellationToken);
            }

            await _dbContext.Characters.Where(c => c.Name == eventObject.CharacterName)
                .ExecuteUpdateAsync(update => update.SetProperty(c => c.DeleteApproachNumber, character.DeleteApproachNumber + 1), cancellationToken);
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
}