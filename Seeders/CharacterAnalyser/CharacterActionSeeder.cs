﻿using Castle.Core.Internal;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Shared.Database.Queries.Sql;
using Shared.Providers;
using TibiaEnemyOtherCharactersFinder.Application.Services;
using TibiaEnemyOtherCharactersFinder.Infrastructure.Entities;

namespace CharacterAnalyserSeeder
{
    public class CharacterActionSeeder : Model
    {
        private readonly TibiaCharacterFinderDbContext _dbContext;
        private readonly IDapperConnectionProvider _connectionProvider;

        public CharacterActionSeeder(TibiaCharacterFinderDbContext dbContext, IDapperConnectionProvider connectionProvider) : base(dbContext)
        {
            _dbContext = dbContext;
            _connectionProvider = connectionProvider;
        }

        public async Task Seed()
        {
            var availableWorlds = await GetAvailableWorldsAsNoTruckingAsync();

            foreach (var availableWorld in availableWorlds)
            {
                var twoWorldScans = await GetFirlsTwoWorldScansAsync(availableWorld.WorldId);

                //var twoWorldScans = worldScans.Take(2).ToList();

                if (twoWorldScans.Count < 2)
                {
                    continue;
                }
                var logoutNames = new List<string>();
                var loginNames = new List<string>();

                logoutNames = GetLogoutNames(twoWorldScans);

                if (!logoutNames.IsNullOrEmpty())
                {
                    loginNames = GetLoginNames(twoWorldScans);
                }

                if (!logoutNames.IsNullOrEmpty() && !loginNames.IsNullOrEmpty())
                {
                    try
                    {
                        await SeedCharactersAsync(logoutNames, loginNames, twoWorldScans);
                        try
                        {
                            await SeedCharacterCorrelationsAsync();
                        }
                        catch (Exception e)
                        {
                            await ClearCharacterActionsAsync();
                            Console.WriteLine(e);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _dbContext.Database.RollbackTransaction();
                    }
                }
                await ClearCharacterActionsAsync();
                SoftDeleteWorldScan(twoWorldScans[0]);
                Console.WriteLine($"{twoWorldScans[0].WorldScanId} - world_id = {twoWorldScans[0].WorldId}");
            }
        }

        private async Task ClearCharacterActionsAsync()
        {
            using (var connection = _connectionProvider.GetConnection(EModuleType.PostgreSql))
            {
                await connection.ExecuteAsync(GenerateQueries.NpgsqlClearCharacterActions);
            }
        }

        private async Task SeedCharacterCorrelationsAsync()
        {
            using (var connection = _connectionProvider.GetConnection(EModuleType.PostgreSql))
            {
                await connection.ExecuteAsync(GenerateQueries.NpgsqlCreateCharacterIfNotExist);
                await connection.ExecuteAsync(GenerateQueries.NpgsqlCreateCharacterCorrelation);
            }
        }

        private List<string> GetLoginNames(List<WorldScan> twoWorldScans) => GetNames(twoWorldScans[1]).Except(GetNames(twoWorldScans[0])).ToList();

        private List<string> GetLogoutNames(List<WorldScan> twoWorldScans) => GetNames(twoWorldScans[0]).Except(GetNames(twoWorldScans[1])).ToList();

        private void SoftDeleteWorldScan(WorldScan worldScan)
        {
            worldScan.IsDeleted = true;
            _dbContext.SaveChanges();
        }

        private async Task SeedCharactersAsync(List<string> logoutNames, List<string> loginNames, List<WorldScan> twoWorldScans)
        {
            _dbContext.Database.BeginTransaction();
            SeedLogoutCharacters(logoutNames, twoWorldScans[0]);
            SeedLoginCharacters(loginNames, twoWorldScans[1]);
            await _dbContext.SaveChangesAsync();
            _dbContext.Database.CommitTransaction();
        }

        private void SeedLogoutCharacters(List<string> logoutNames, WorldScan worldScan)
        {
            var logoutCharacters = new List<CharacterAction>();
            foreach (var logoutName in logoutNames)
            {
                var logoutCharacter = CreateCharacterLogoutOrLogin(logoutName, false, worldScan);
                logoutCharacters.Add(logoutCharacter);
            }
            _dbContext.CharacterActions.AddRange(logoutCharacters);
        }

        private void SeedLoginCharacters(List<string> loginNames, WorldScan worldScan)
        {
            var loginCharacters = new List<CharacterAction>();
            foreach (var loginName in loginNames)
            {
                var loginCharacter = CreateCharacterLogoutOrLogin(loginName, true, worldScan);
                loginCharacters.Add(loginCharacter);
            }
            _dbContext.CharacterActions.AddRange(loginCharacters);
        }

        private CharacterAction CreateCharacterLogoutOrLogin(string characterName, bool isOnline, WorldScan worldScan)
        {
            return new CharacterAction()
            {
                CharacterName = characterName,
                WorldScanId = worldScan.WorldScanId,
                WorldId = worldScan.WorldId,
                IsOnline = isOnline
            };
        }

        private IQueryable<WorldScan> GetFirstTwoWorldScansFromSpecificWorld(short world)
        {
            try
            {
                return _dbContext.WorldScans.Where(w => w.WorldId == world && !w.IsDeleted)
                    .OrderBy(w => w.WorldScanId).Take(2).AsNoTracking();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetNames(WorldScan worldScan)
        {
            var names = worldScan.CharactersOnline.Split("|").ToList();
            names.RemoveAll(string.IsNullOrWhiteSpace);
            return names;
        }
    }
}
