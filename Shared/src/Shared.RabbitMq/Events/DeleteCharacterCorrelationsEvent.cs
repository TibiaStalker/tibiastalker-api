namespace Shared.RabbitMQ.Events;

/// <summary>
/// Event to delete all correlations
/// </summary>
/// <param name="CharacterName"></param>
public record DeleteCharacterCorrelationsEvent(string CharacterName) : IntegrationEvent;
