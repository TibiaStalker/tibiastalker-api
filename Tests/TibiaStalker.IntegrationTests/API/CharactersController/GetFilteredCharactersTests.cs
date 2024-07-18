using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using TibiaStalker.Application.Dtos;
using TibiaStalker.IntegrationTests.Configuration;

namespace TibiaStalker.IntegrationTests.API.CharactersController;

[Collection(TestCollections.ApiCollection)]
public class GetFilteredCharactersTests : CharactersTestBase
{
    private const string ControllerBase = "api/tibia-stalker/v1/characters";

    public GetFilteredCharactersTests(TibiaApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetFilteredCharactersEndpoint_WithParametersThatFitsToDataInDatabase_ShouldReturnsCorrectData()
    {
        // Arrange
        var searchText = "name";
        var page = 1;
        var pageSize = 10;
        var queryParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var response = await Client.GetAsync($"{ControllerBase}{queryParameters}");
        var result = await response.Content.ReadFromJsonAsync<FilteredCharactersDto>();

        // Assert
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(6);
    }
    
    [Fact]
    public async Task GetFilteredCharactersEndpoint_WithSearchTextThatNotFoundInDatabase_ShouldReturnStatusNotFound()
    {
        // Arrange
        var searchText = "not-found";
        var page = 1;
        var pageSize = 10;
        var queryParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var response = await Client.GetAsync($"{ControllerBase}{queryParameters}");
        var content = await response.Content.ReadAsStringAsync();
        var contentDeserialized = JsonConvert.DeserializeObject<FilteredCharactersDto>(content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        contentDeserialized!.TotalCount.Should().Be(0);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLengthRouteParameters))]
    public async Task GetFilteredCharactersEndpoint_WithInvalidLengthSearchText_ShouldReturnsStatusBadRequest(string parameter)
    {
        // Arrange
        var searchText = parameter;
        var page = 1;
        var pageSize = 10;
        var queryParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var result = await Client.GetAsync($"{ControllerBase}{queryParameters}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetUnacceptableRouteParameters))]
    public async Task GetFilteredCharactersEndpoint_WithUnacceptableCharactersInSearchText_ShouldReturnsStatusBadRequest(string parameter)
    {
        // Arrange
        var searchText = parameter;
        var page = 1;
        var pageSize = 10;
        var queryParameters = $"?{nameof(searchText)}={searchText}&{nameof(page)}={page}&{nameof(pageSize)}={pageSize}";

        // Act
        var result = await Client.GetAsync($"{ControllerBase}{queryParameters}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(GetUnacceptablePages))]
    public async Task GetFilteredCharactersEndpoint_WithUnacceptablePage_ShouldReturnsStatusBadRequest(int parameter)
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
    public async Task GetFilteredCharactersEndpoint_WithUnacceptablePageSize_ShouldReturnsStatusBadRequest(int parameter)
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