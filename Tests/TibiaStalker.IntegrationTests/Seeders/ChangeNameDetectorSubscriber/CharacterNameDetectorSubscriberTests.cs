using ChangeNameDetector.Services;
using ChangeNameDetectorSubscriber.Subscribers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RichardSzalay.MockHttp;
using Shared.RabbitMQ.Initializers;
using TibiaStalker.Infrastructure.Persistence;
using TibiaStalker.IntegrationTests.Configuration;
using TibiaStalker.IntegrationTests.Seeders.DatabaseSeeders;

namespace TibiaStalker.IntegrationTests.Seeders.ChangeNameDetectorSubscriber;

[Collection(TestCollections.RabbitCollection)]
public class CharacterNameDetectorSubscriberTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;
    private readonly string _startOfFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TestCommons.TibiaDataResponsesFolder, "TibiaDataCharacter");
    private const string TibiaDataCharacterEndpoint = "/v3/character/";
    private const string NameD = "name-d";
    private const string NameG = "name-g";
    private const int CharacterId = 111;
    private const int DelayMilliseconds = 500;

    public CharacterNameDetectorSubscriberTests(TibiaSeederFactory factory)
    {
        _factory = factory;
        var rabbitMqInitializer = _factory.Services.GetRequiredService<InitializationRabbitMqTaskRunner>();
        rabbitMqInitializer.StartAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task CharacterNameDetectorSubscriber_SubscribeWithExistingCharacter_ShouldUpdateVerifiedDate()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorSubscriber>();

        var dbContext = _factory.Services.GetRequiredService<ITibiaStalkerDbContext>();

        var filePath = Path.Combine(_startOfFilePath, "GetExistingCharacterResponse.json");
        var getCharactersResponse = await File.ReadAllTextAsync(filePath);

        _factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameD}")
            .Respond("application/json", getCharactersResponse);

        while (await changeNameDetector.Run())
        {
            // To run for all characters
        }


        // Act
        await Subscribe(subscriber);


        // Assert
        var character = dbContext.Characters.AsNoTracking().First(ch => ch.CharacterId == CharacterId);

        character.TradedDate.Should().Be(new DateOnly(2001, 1, 1));
        character.VerifiedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
    }

    [Fact]
    public async Task CharacterNameDetectorSubscriber_SubscribeWithChangedNameCharacter_ShouldUpdateName()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorSubscriber>();

        var dbContext = _factory.Services.GetRequiredService<ITibiaStalkerDbContext>();

        var filePath = Path.Combine(_startOfFilePath, "GetExistingCharacterWithChangedNameResponse.json");
        var getCharacterResponse = await File.ReadAllTextAsync(filePath);

        _factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameD}")
            .Respond("application/json", getCharacterResponse);

        while (await changeNameDetector.Run())
        {
            // To run for all characters
        }


        // Act
        await Subscribe(subscriber);


        // Assert
        var character = dbContext.Characters.AsNoTracking().First(ch => ch.CharacterId == CharacterId);

        character.Name.Should().Be("name-dd");
        character.TradedDate.Should().Be(new DateOnly(2001, 1, 1));
        character.VerifiedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
    }

    [Fact]
    public async Task CharacterNameDetectorSubscriber_SubscribeWithChangedNameCharacterToExistingName_ShouldMergeCorrelations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorSubscriber>();

        var dbContext = _factory.Services.GetRequiredService<ITibiaStalkerDbContext>();

        var filePath = Path.Combine(_startOfFilePath, "GetExistingCharacterWithChangedNameToExistingNameResponse.json");
        var getCharactersResponse = await File.ReadAllTextAsync(filePath);

        _factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameD}")
            .Respond("application/json", getCharactersResponse);

        while (await changeNameDetector.Run())
        {
            // To run for all characters
        }


        // Act
        await Subscribe(subscriber);


        // Assert
        var characters = dbContext.Characters.AsNoTracking().ToList();
        var correlations = dbContext.CharacterCorrelations.AsNoTracking().ToList();

        characters.Count.Should().Be(3);
        characters.First(c => c.Name == "name-f").VerifiedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

        correlations.Count.Should().Be(2);
        correlations.First(c => c.LoginCharacterId == 112 || c.LogoutCharacterId == 112).NumberOfMatches.Should().Be(20);
    }

    [Fact]
    public async Task CharacterNameDetectorSubscriber_SubscribeWithNotExistCharacter_ShouldUpdateDeleteApproach()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorSubscriber>();

        var dbContext = _factory.Services.GetRequiredService<ITibiaStalkerDbContext>();

        var filePath = Path.Combine(_startOfFilePath, "GetNotExistingCharacterResponse.json");
        var getCharactersResponse = await File.ReadAllTextAsync(filePath);

        _factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameD}")
            .Respond("application/json", getCharactersResponse);

        while (await changeNameDetector.Run())
        {
            // To run for all characters
        }


        // Act
        await Subscribe(subscriber);


        // Assert
        var character = dbContext.Characters.AsNoTracking().First(ch => ch.CharacterId == CharacterId);

        character.Name.Should().Be("name-d");
        character.TradedDate.Should().Be(new DateOnly(2001, 1, 1));
        character.VerifiedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
        character.DeleteApproachNumber.Should().Be(1);
    }

    [Fact]
    public async Task CharacterNameDetectorSubscriber_SubscribeWithNotExistCharacter_ShouldDeleteCharacterWithCorrelations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorSubscriber>();

        var dbContext = _factory.Services.GetRequiredService<ITibiaStalkerDbContext>();

        var filePath = Path.Combine(_startOfFilePath, "GetNotExistingCharacterResponse.json");
        var getCharactersResponse = await File.ReadAllTextAsync(filePath);

        _factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameG}")
            .Respond("application/json", getCharactersResponse);

        while (await changeNameDetector.Run())
        {
            // To run for all characters
        }


        // Act
        await Subscribe(subscriber);


        // Assert
        var character = dbContext.Characters.AsNoTracking().ToList();
        var characterCorrelations = dbContext.CharacterCorrelations.AsNoTracking().ToList();

        character.Count.Should().Be(3);
        characterCorrelations.Count.Should().Be(2);
    }

    [Fact]
    public async Task CharacterNameDetectorSubscriber_SubscribeWithTradedCharacter_ShouldDeleteCorrelations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var changeNameDetector = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorService>();
        var subscriber = scope.ServiceProvider.GetRequiredService<IChangeNameDetectorSubscriber>();
        var dbContext = _factory.Services.GetRequiredService<ITibiaStalkerDbContext>();

        var filePath = Path.Combine(_startOfFilePath, "GetTradedCharacterResponse.json");
        var getCharactersResponse = await File.ReadAllTextAsync(filePath);

        _factory.MockHttp.When($"{TibiaDataCharacterEndpoint}{NameD}")
            .Respond("application/json", getCharactersResponse);

        while (await changeNameDetector.Run())
        {
            // To run for all characters
        }


        // Act
        await Subscribe(subscriber);


        // Assert
        var character = dbContext.Characters.AsNoTracking().First(ch => ch.CharacterId == CharacterId);
        var characterCorrelations = dbContext.CharacterCorrelations.AsNoTracking().ToList();

        character.Name.Should().Be("name-d");
        character.TradedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));
        character.VerifiedDate.Should().Be(DateOnly.FromDateTime(DateTime.Now));

        characterCorrelations.Count.Should().Be(2);
    }

    public async Task InitializeAsync()
    {
        _factory.MockHttp.Clear();
       await _factory.ResetDatabaseAsync<TestDatabaseSeederCharacterNameDetector>();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private static async Task Subscribe(IChangeNameDetectorSubscriber subscriber)
    {
        subscriber.Subscribe();
        await Task.Delay(DelayMilliseconds);
        subscriber.CloseChannels();
    }
}