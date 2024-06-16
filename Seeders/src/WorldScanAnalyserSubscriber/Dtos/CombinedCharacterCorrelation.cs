using TibiaStalker.Domain.Entities;

namespace WorldScanAnalyserSubscriber.Dtos;

public class CombinedCharacterCorrelation
{
    public CharacterCorrelation FirstCombinedCorrelation { get; set; }
    public CharacterCorrelation SecondCombinedCorrelation { get; set; }
}