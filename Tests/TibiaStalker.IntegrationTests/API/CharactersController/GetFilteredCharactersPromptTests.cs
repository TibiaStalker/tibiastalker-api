using System.Net;
using FluentAssertions;
using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.API.CharactersController;

[Collection(TestCollections.ApiCollection)]
public class GetFilteredCharactersPromptTests : CharactersTestBase
{
    private const string ControllerBase = "api/tibia-stalker/v1/characters/prompt";

    public GetFilteredCharactersPromptTests(TibiaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetFilteredCharactersPromptEndpoint_WithParametersThatFitsToDataInDatabase_ShouldReturnsCorrectData()
    {
        // Arrange
        var searchText = "name";
        var page = 1;
        var pageSize = 10;
        var additionalParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var response = await Client.GetAsync($"{ControllerBase}{additionalParameters}");
        var result = await response.Content.ReadFromJsonAsync<List<string>>();

        // Assert
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThan(0);
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidLengthRouteParameters))]
    public async Task GetFilteredCharactersPromptEndpoint_WithInvalidLengthSearchText_ShouldReturnsStatusBadRequest(string parameter)
    {
        // Arrange
        var searchText = parameter;
        var page = 1;
        var pageSize = 10;
        var additionalParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var result = await Client.GetAsync($"{ControllerBase}{additionalParameters}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetUnacceptableRouteParameters))]
    public async Task GetFilteredCharactersPromptEndpoint_WithUnacceptableCharactersInSearchText_ShouldReturnsStatusBadRequest(string parameter)
    {
        // Arrange
        var searchText = parameter;
        var page = 1;
        var pageSize = 10;
        var additionalParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var result = await Client.GetAsync($"{ControllerBase}{additionalParameters}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetUnacceptablePages))]
    public async Task GetFilteredCharactersPromptEndpoint_WithUnacceptablePage_ShouldReturnsStatusBadRequest(int parameter)
    {
        // Arrange
        var searchText = "name";
        var page = parameter;
        var pageSize = 10;
        var additionalParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var result = await Client.GetAsync($"{ControllerBase}{additionalParameters}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetUnacceptablePageSizes))]
    public async Task GetFilteredCharactersPromptEndpoint_WithUnacceptablePageSize_ShouldReturnsStatusBadRequest(int parameter)
    {
        // Arrange
        var searchText = "name";
        var page = 1;
        var pageSize = parameter;
        var additionalParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var result = await Client.GetAsync($"{ControllerBase}{additionalParameters}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}