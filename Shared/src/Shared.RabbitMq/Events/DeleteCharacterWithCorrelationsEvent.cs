namespace Shared.RabbitMQ.Events;

/// <summary>
/// Event to delete character with all correlations
/// </summary>
/// <param name="CharacterName"></param>
public record DeleteCharacterWithCorrelationsEvent(string CharacterName) : IntegrationEvent;
