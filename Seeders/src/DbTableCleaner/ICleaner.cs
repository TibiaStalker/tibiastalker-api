namespace DbCleaner;

public interface ICleaner
{
    Task ClearUnnecessaryWorldScans();
    Task DeleteIrrelevantCharacterCorrelations();
    Task VacuumWorldScans();
    Task VacuumCharacters();
    Task VacuumWorlds();
    Task VacuumCharacterCorrelations();
}