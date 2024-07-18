namespace TibiaStalker.Domain.Entities;

public class CharacterCorrelation : IEntity
{
    /// <summary>
    /// ID of specific correlation between two characters
    /// </summary>
    public long CorrelationId { get; set; }

    /// <summary>
    /// ID of specific character that logout
    /// </summary>
    public int LogoutCharacterId { get; set; }

    /// <summary>
    /// ID of specific character that login
    /// </summary>
    public int LoginCharacterId { get; set; }

    /// <summary>
    /// Quantity occurrence of combination
    /// </summary>
    public int NumberOfMatches { get; set; }

    /// <summary>
    /// Date of first occurence
    /// </summary>
    public DateOnly CreateDate { get; set; }

    /// <summary>
    /// Date of last occurence
    /// </summary>
    public DateOnly LastMatchDate { get; set; }

    // Associations
    public Character LogoutCharacter { get; set; }
    public Character LoginCharacter { get; set; }
}
