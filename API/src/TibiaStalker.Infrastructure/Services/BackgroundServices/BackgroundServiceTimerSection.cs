using System.ComponentModel.DataAnnotations;

namespace TibiaStalker.Infrastructure.Services.BackgroundServices;

public class BackgroundServiceTimerSection
{
    public const string SectionName = "BackgroundServiceTimer";

    [Required]
    public int CharacterTrackNotifier { get; init; }
    [Required]
    public int OnlineCharactersScannerWorker { get; init; }
}