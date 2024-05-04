using System.ComponentModel.DataAnnotations;

namespace TibiaStalker.Application.Configuration.Settings;

public class SeederVariablesSection
{
    public const string SectionName = "SeederVariables";
    public ChangeNameDetectorSection ChangeNameDetector { get; init; }
}

public class ChangeNameDetectorSection
{
    [Required] public int VisibilityOfTradePeriod { get; init; }
    [Required] public int ScanPeriod { get; init; }
}