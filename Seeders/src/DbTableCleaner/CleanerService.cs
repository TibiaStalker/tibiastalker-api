using DbCleaner.Decorators;
using Microsoft.Extensions.Options;
using TibiaStalker.Application.Configuration.Settings;

namespace DbCleaner;

public class CleanerService : ICleanerService
{
    private readonly ICleaner _cleaner;
    private readonly IDbCleanerLogDecorator _logDecorator;
    private readonly DbCleanerSection _dbCleanerOptions;


    public CleanerService(ICleaner cleaner, IDbCleanerLogDecorator logDecorator, IOptions<SeederVariablesSection> options)
    {
        _cleaner = cleaner;
        _logDecorator = logDecorator;
        _dbCleanerOptions = options.Value.DbCleaner;
    }

    public async Task Run()
    {
        await (_dbCleanerOptions.IsEnableClearUnnecessaryWorldScans ? _logDecorator.Decorate(_cleaner.ClearUnnecessaryWorldScans) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableTruncateCharacterActions ? _logDecorator.Decorate(_cleaner.TruncateCharacterActions) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableDeleteIrrelevantCharacterCorrelations ? _logDecorator.Decorate(_cleaner.DeleteIrrelevantCharacterCorrelations) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableVacuumCharacterActions ? _logDecorator.Decorate(_cleaner.VacuumCharacterActions) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableVacuumWorldScans ? _logDecorator.Decorate(_cleaner.VacuumWorldScans) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableVacuumCharacters ? _logDecorator.Decorate(_cleaner.VacuumCharacters) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableVacuumWorlds ? _logDecorator.Decorate(_cleaner.VacuumWorlds) : Task.CompletedTask);
        await (_dbCleanerOptions.IsEnableVacuumCharacterCorrelations ? _logDecorator.Decorate(_cleaner.VacuumCharacterCorrelations) : Task.CompletedTask);
    }
}