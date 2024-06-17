namespace Shared.RabbitMQ.Events;

/// <summary>
/// Event to detect changes in character
/// </summary>
/// <param name="CharacterName"></param>
public record ChangeNameDetectorEvent(string CharacterName) : IntegrationEvent;
