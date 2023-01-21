﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TibiaEnemyOtherCharactersFinder.Infrastructure.Entities;

namespace TibiaEnemyOtherCharactersFinder.Infrastructure.Configuration;

public class DataBaseInitializer : IInitializer
{
    private readonly ILogger<DataBaseInitializer> _logger;
    private readonly TibiaCharacterFinderDbContext _dbContext;

    public DataBaseInitializer(ILogger<DataBaseInitializer> logger, TibiaCharacterFinderDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public int? Order => 1;

    public async Task Initialize()
    {
        _logger.LogInformation("Database technology initializer - started");
        await _dbContext.Database.MigrateAsync();
        _logger.LogInformation("Database technology initializer - finished");
    }
}