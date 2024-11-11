using FluentAssertions;
using TibiaStalker.Application.Dtos;
using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.API.WorldsController;

[Collection(TestCollections.ApiCollection)]
public class GetWorldsTests
{
    private const string ControllerBase = "api/tibia-stalker/v1/worlds";
    private readonly HttpClient _client;

    public GetWorldsTests(TibiaApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorldsEndpoint_WithTrueInParameter_ShouldReturnsAvailableWorlds()
    {
        // Arrange
        var isAvailable = true;
        var queryParameters = $"{nameof(isAvailable)}={isAvailable}";

        // Act
        var response = await _client.GetAsync($"{ControllerBase}?{queryParameters}");
        var result = await response.Content.ReadFromJsonAsync<GetWorldsResult>();

        // Assert
        result.Should().NotBeNull();
        result!.Worlds.Count.Should().Be(2);
        result.Worlds[0].Name.Should().Be("World-A");
        result.Worlds[1].Name.Should().Be("World-B");
    }

    [Fact]
    public async Task GetWorldsEndpoint_WithFalseInParameter_ShouldReturnsUnavailableWorlds()
    {
        // Arrange
        var isAvailable = false;
        var additionalParameters = $"?{nameof(isAvailable)}={isAvailable}";

        // Act
        var response = await _client.GetAsync($"{ControllerBase}{additionalParameters}");
        var result = await response.Content.ReadFromJsonAsync<GetWorldsResult>();

        // Assert
        result.Should().NotBeNull();
        result!.Worlds.Count.Should().Be(1);
        result.Worlds[0].Name.Should().Be("World-C");
    }

    [Fact]
    public async Task GetWorldsEndpoint_WithoutParameter_ShouldReturnsAllWorlds()
    {
        // Arrange
        // Act
        var response = await _client.GetAsync($"{ControllerBase}");
        var result = await response.Content.ReadFromJsonAsync<GetWorldsResult>();

        // Assert
        result.Should().NotBeNull();
        result!.Worlds.Count.Should().Be(3);
        result.Worlds[0].Name.Should().Be("World-A");
        result.Worlds[1].Name.Should().Be("World-B");
        result.Worlds[2].Name.Should().Be("World-C");
    }
}