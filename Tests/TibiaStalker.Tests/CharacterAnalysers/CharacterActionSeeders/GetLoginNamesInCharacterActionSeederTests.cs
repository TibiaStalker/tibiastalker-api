using Moq;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace Seeders.Tests.CharacterAnalysers.CharacterActionSeeders;

public class GetLoginNamesInCharacterActionSeederTests
{
    private readonly Mock<ITibiaStalkerDbContext> _dbContextMock1 = new();
    private readonly Mock<ITibiaStalkerDbContext> _dbContextMock2 = new();

    public GetLoginNamesInCharacterActionSeederTests()
    {
    }
    
    [Fact]
    public void GetLoginNamesInCharacterActionSeederReturnsExpectedResult()
    {
        // Arrange
        var worldScans = new List<WorldScan>
        {
            new() { WorldScanId = 3217, WorldId = 1, ScanCreateDateTime = new DateTime(2022,11,30,20,23,12, DateTimeKind.Utc), CharactersOnline = "aphov|armystrong|asiier|braws|burntmeat|fosani|friedbert|ganancia adra|guga falido|just mojito|kinaduh|kineador|kiperr the third"},
            new() { WorldScanId = 3302, WorldId = 1, ScanCreateDateTime = new DateTime(2022,11,30,20,28,36, DateTimeKind.Utc), CharactersOnline = "aphov|armystrong|asiier|braws|brytiaggo|fresita linda|friedbert|ganancia adra|guga falido|just mojito|kinaduh|kineador"},
        };
        // var characterActionSeeder = new CharacterActionsManager();
        // characterActionSeeder.SetFirstAndSecondScanNames(worldScans);
        
        // Act
        // var loginNames = characterActionSeeder.GetAndSetLoginNames();

        // Assert
        // loginNames.Count.Should().Be(2);
    }
    
    [Fact]
    public void GetLoginNamesInCharacterActionSeederReturnsEmptyListIfEveryNameFromSecondWorldScanExistInFirstWorldScan()
    {
        // Arrange
        var worldScans = new List<WorldScan>
        {
            new() { WorldScanId = 3217, WorldId = 1, ScanCreateDateTime = new DateTime(2022,11,30,20,23,12, DateTimeKind.Utc), CharactersOnline = "aphov|armystrong|asiier|braws|burntmeat|fosani|friedbert|ganancia adra|guga falido|just mojito|kinaduh|kineador|kiperr the third"},
            new() { WorldScanId = 3302, WorldId = 1, ScanCreateDateTime = new DateTime(2022,11,30,20,28,36, DateTimeKind.Utc), CharactersOnline = "aphov|armystrong|asiier|braws|ganancia adra|guga falido|just mojito|kinaduh|kineador"},
        };
        // var characterActionSeeder = new CharacterActionsManager(_dbContextMock1.Object, _dbContextMock2.Object);
        // characterActionSeeder.SetFirstAndSecondScanNames(worldScans);

        // Act
        // var loginNames = characterActionSeeder.GetAndSetLoginNames();

        // Assert
        // loginNames.Count.Should().Be(0);
    }
}