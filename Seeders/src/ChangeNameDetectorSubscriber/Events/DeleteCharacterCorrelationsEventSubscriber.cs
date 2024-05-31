using System.Text;
using ChangeNameDetectorSubscriber.Handlers;
using ChangeNameDetectorSubscriber.Subscribers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Shared.RabbitMQ.Conventions;
using Shared.RabbitMQ.Events;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace ChangeNameDetectorSubscriber.Events;

public class DeleteCharacterCorrelationsEventSubscriber : IEventSubscriber
{
    private readonly ILogger<DeleteCharacterCorrelationsEventSubscriber> _logger;
    private readonly IEventResultHandler _eventResultHandler;
    private readonly IRabbitMqConventionProvider _conventionProvider;
    private readonly ITibiaStalkerDbContext _dbContext;

    public DeleteCharacterCorrelationsEventSubscriber(
        ILogger<DeleteCharacterCorrelationsEventSubscriber> logger,
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
        var queueBinding = _conventionProvider.GetForType<DeleteCharacterCorrelationsEvent>();
        return queueBinding.Queue;
    }

    public async Task OnReceived(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        var payload = Encoding.UTF8.GetString(ea.Body.Span);
        var eventObject = JsonConvert.DeserializeObject<DeleteCharacterCorrelationsEvent>(payload);
        _logger.LogInformation("Event {Event} subscribed. Payload: {Payload}", eventObject.GetType().Name, payload);

        Thread.Sleep(1000);

        var character = new Character();

        var isCommitedProperly = await ExecuteInTransactionAsync(Action, payload);

        _eventResultHandler.HandleTransactionResult(isCommitedProperly, nameof(DeleteCharacterCorrelationsEvent), payload, character.Name);
        return;

        async Task Action()
        {
            character = await _dbContext.Characters.Where(c => c.Name == eventObject.CharacterName)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            await _dbContext.CharacterCorrelations.Where(c => c.LoginCharacterId == character.CharacterId || c.LogoutCharacterId == character.CharacterId)
                .ExecuteDeleteAsync(cancellationToken);

            await _dbContext.Characters.Where(c => c.CharacterId == character.CharacterId)
                .ExecuteUpdateAsync(update => update.SetProperty(c => c.TradedDate, DateOnly.FromDateTime(DateTime.Now)), cancellationToken);
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