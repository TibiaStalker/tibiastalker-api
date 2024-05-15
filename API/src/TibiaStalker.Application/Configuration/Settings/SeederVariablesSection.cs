using System.ComponentModel.DataAnnotations;

namespace TibiaStalker.Application.Configuration.Settings;

public class SeederVariablesSection
{
    public const string SectionName = "SeederVariables";
    public ChangeNameDetectorSection ChangeNameDetector { get; init; }
    public DbCleanerSection DbCleaner { get; init; }
}

public class ChangeNameDetectorSection
{
    [Required] public int VisibilityOfTradePeriod { get; init; }
    [Required] public int ScanPeriod { get; init; }
}

public class DbCleanerSection
{
    [Required] public bool IsEnableClearUnnecessaryWorldScans { get; init; }
    [Required] public bool IsEnableTruncateCharacterActions { get; init; }
    [Required] public bool IsEnableDeleteIrrelevantCharacterCorrelations { get; init; }
    [Required] public bool IsEnableVacuumCharacterActions { get; init; }
    [Required] public bool IsEnableVacuumWorldScans { get; init; }
    [Required] public bool IsEnableVacuumCharacters { get; init; }
    [Required] public bool IsEnableVacuumWorlds { get; init; }
    [Required] public bool IsEnableVacuumCharacterCorrelations { get; init; }
}