using System.ComponentModel.DataAnnotations;

namespace TibiaStalker.Infrastructure.Clients.TibiaData;

public class TibiaDataSection
{
    public const string SectionName = "TibiaData";

    [Required]
    public string BaseAddress { get; init; }
    [Required]
    public string ApiVersion { get; init; }
    [Required]
    public string Timeout { get; init; }
}