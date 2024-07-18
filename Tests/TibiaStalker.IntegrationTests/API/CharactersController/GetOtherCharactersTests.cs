using System.Net;
using FluentAssertions;
using RichardSzalay.MockHttp;
using TibiaStalker.Application.Dtos;
using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.API.CharactersController;

[Collection(TestCollections.ApiCollection)]
public class GetOtherCharactersTests : CharactersTestBase
{
    private readonly string _startOfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TestCommons.TibiaDataResponsesFolder);
    private const string ControllerBase = "api/tibia-stalker/v1/characters";
    private const string TibiaDataCharacterEndpoint = "/v3/character/";
    private const string NameNotFound = "name-not-found";
    private const string NameInDatabase = "name-a";

    public GetOtherCharactersTests(TibiaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetOtherCharactersEndpoint_WithRouteParametersThatFoundInDatabase_ShouldReturnsCorrectData()
    {
        // Arrange
        var filePath = Path.Combine(_startOfFilePath, "CharactersController", "GetCharacterResponse.json");
        var getCharactersResponse = await File.ReadAllTextAsync(filePath);

        Factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameInDatabase}")
            .Respond("application/json", getCharactersResponse);

        // Act
        var response = await Client.GetAsync($"{ControllerBase}/{NameInDatabase}");
        var result = await response.Content.ReadFromJsonAsync<CharacterWithCorrelationsResult>();

        // Assert
        result.Should().NotBeNull();
        result!.PossibleInvisibleCharacters.Count.Should().Be(2);
        result.PossibleInvisibleCharacters.First(c => c.OtherCharacterName == "name-b").NumberOfMatches.Should().Be(8);
    }

    [Fact]
    public async Task GetOtherCharactersEndpoint_WithRouteParametersThatNotFoundInDatabase_ShouldReturnStatusNotFound()
    {
        // Arrange
        var filePath = Path.Combine(_startOfFilePath, "CharactersController", "CharacterNotFoundResponse.json");
        var characterNotFoundResponse = await File.ReadAllTextAsync(filePath);

        Factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameNotFound}")
            .Respond("application/json", characterNotFoundResponse);

        // Act
        var result = await Client.GetAsync($"{ControllerBase}/{NameNotFound}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLengthRouteParameters))]
    public async Task GetOtherCharactersEndpoint_WithInvalidLengthRouteParameters_ShouldReturnsStatusBadRequest(string parameter)
    {
        // Arrange
        // Act
        var result = await Client.GetAsync($"{ControllerBase}/{parameter}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetUnacceptableRouteParameters))]
    public async Task GetOtherCharactersEndpoint_WithUnacceptableCharactersInRouteParameters_ShouldReturnsStatusBadRequest(string parameter)
    {
        // Arrange
        // Act
        var result = await Client.GetAsync($"{ControllerBase}/{parameter}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}