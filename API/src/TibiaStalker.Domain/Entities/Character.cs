namespace TibiaStalker.Domain.Entities;

public class Character : IEntity
{
    /// <summary>
    /// ID of specific character
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// Character name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// ID of specific world
    /// </summary>
    public short WorldId { get; set; }

    /// <summary>
    /// Date of verified character when changed name
    /// </summary>
    public DateOnly? VerifiedDate { get; set; }

    /// <summary>
    /// Approximately date of trade character
    /// </summary>
    public DateOnly? TradedDate { get; set; }

    /// <summary>
    /// Number of character delete approach
    /// </summary>
    public int DeleteApproachNumber { get; set; }

    // Associations
    public World World { get; set; }
    public List<CharacterCorrelation> LogoutCharacterCorrelations { get; set; }
    public List<CharacterCorrelation> LoginCharacterCorrelations { get; set; }
}
