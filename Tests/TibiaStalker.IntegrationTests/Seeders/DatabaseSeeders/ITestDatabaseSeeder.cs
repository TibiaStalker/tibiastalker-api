namespace TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

public interface ITestDatabaseSeeder
{
    public Task ResetDatabaseAsync();
}