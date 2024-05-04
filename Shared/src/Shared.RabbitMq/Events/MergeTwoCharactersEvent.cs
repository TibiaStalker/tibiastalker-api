namespace Shared.RabbitMQ.Events;

/// <summary>
/// Event to merge correlations of old and new character
/// </summary>
/// <param name="OldCharacterName"></param>
/// <param name="NewCharacterName"></param>
public record MergeTwoCharactersEvent(string OldCharacterName, string NewCharacterName) : IntegrationEvent;
