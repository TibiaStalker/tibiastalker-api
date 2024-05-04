﻿using System.Text;
using ChangeNameDetector.Services;
using ChangeNameDetector.Validators;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Shared.RabbitMQ.Configuration;
using Shared.RabbitMq.Conventions;
using Shared.RabbitMQ.EventBus;
using Shared.RabbitMQ.Events;
using Shared.RabbitMQ.Initializers;
using TibiaStalker.Application.Configuration.Settings;
using TibiaStalker.Application.Interfaces;
using TibiaStalker.Application.TibiaData.Dtos;
using TibiaStalker.Infrastructure.Persistence;

namespace Seeders.IntegrationTests.ChangeNameDetector;

[Collection("Seeder test collection")]
public class CharacterNameDetectorTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;
    private readonly Func<Task> _resetDatabase;
    private readonly Mock<ITibiaDataClient> _tibiaDataClientMock = new();

    public CharacterNameDetectorTests(TibiaSeederFactory factory)
    {
        _factory = factory;
        _resetDatabase = factory.ResetDatabaseAsync;
    }
    
    [Fact]
    public async Task Run_WhenCharacterExistAndWasNotTraded_ShouldOnlyUpdateVerifiedDate()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var dbContextForMock = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChangeNameDetectorService>>();
        var busPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dbContextBefore = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<INameDetectorValidator>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeederVariablesSection>>();


        var charactersBeforeDetector = dbContextForMock.Characters.AsNoTracking().ToList();

        foreach (var character in charactersBeforeDetector)
        {
            _tibiaDataClientMock.Setup(r => r.FetchCharacter(character.Name, false)).ReturnsAsync(PrepareExistingTibiaDataCharacter(character.Name));
        }

        var changeNameDetector = new ChangeNameDetectorService(logger, validator, dbContextBefore, _tibiaDataClientMock.Object, busPublisher, options);


        // Act
        await changeNameDetector.Run();


        // Assert
        var dbContextAfter = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var charactersAfterDetector = dbContextAfter.Characters.AsNoTracking().ToList();

        charactersAfterDetector.Select(c => c.VerifiedDate).Should().AllBeEquivalentTo(DateOnly.FromDateTime(DateTime.Now));
        charactersAfterDetector.Count.Should().Be(4);
        charactersBeforeDetector.Select(c => c.VerifiedDate).Should().OnlyContain(date => date == null);
    }

    [Fact]
    public async Task Run_WhenCharacterNotExist_ShouldSendEventDeleteCharacterWithCorrelationsEvent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChangeNameDetectorService>>();
        var busPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<INameDetectorValidator>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeederVariablesSection>>();


        var charactersBeforeDetector = dbContext.Characters.AsNoTracking().ToList();

        for (var i = 0; i < charactersBeforeDetector.Count; i++)
        {
            SetupTibiaDataServiceMock(charactersBeforeDetector[i].Name, (i < 2, PrepareNonExistingTibiaDataCharacter));
        }

        var changeNameDetector = new ChangeNameDetectorService(logger, validator, dbContext, _tibiaDataClientMock.Object, busPublisher, options);


        // Act
        await changeNameDetector.Run();


        // Assert
        var receivedObjects = SubscribeRabbitMessagesFromQueue<DeleteCharacterWithCorrelationsEvent>();

        var charactersAfterDetector = dbContext.Characters.AsNoTracking().ToList();


        charactersAfterDetector.Select(c => c.VerifiedDate).Should().AllBeEquivalentTo(DateOnly.FromDateTime(DateTime.Now));
        charactersAfterDetector.Count.Should().Be(4);
        charactersBeforeDetector.Select(c => c.VerifiedDate).Should().OnlyContain(date => date == null);

        receivedObjects.Select(o => o.CharacterName).Should().Contain(new[] { "aphov", "asiier" });
        receivedObjects.Count.Should().Be(2);
    }

    [Fact]
    public async Task Run_WhenCharacterWasTraded_ShouldSendEventDeleteChcaracterCorrelationsEvent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChangeNameDetectorService>>();
        var busPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<INameDetectorValidator>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeederVariablesSection>>();


        var charactersBeforeDetector = dbContext.Characters.AsNoTracking().ToList();

        for (var i = 0; i < charactersBeforeDetector.Count; i++)
        {
            SetupTibiaDataServiceMock(charactersBeforeDetector[i].Name, (i < 2, () => PrepareTradedTibiaDataCharacter(charactersBeforeDetector[i].Name)));
        }

        var changeNameDetector = new ChangeNameDetectorService(logger, validator, dbContext, _tibiaDataClientMock.Object, busPublisher, options);


        // Act
        await changeNameDetector.Run();


        // Assert
        var receivedObjects = SubscribeRabbitMessagesFromQueue<DeleteCharacterCorrelationsEvent>();

        var charactersAfterDetector = dbContext.Characters.AsNoTracking().ToList();


        charactersAfterDetector.Select(c => c.VerifiedDate).Should().AllBeEquivalentTo(DateOnly.FromDateTime(DateTime.Now));
        charactersAfterDetector.Count.Should().Be(4);
        charactersBeforeDetector.Select(c => c.VerifiedDate).Should().OnlyContain(date => date == null);

        receivedObjects.Select(o => o.CharacterName).Should().Contain(new[] { "aphov", "asiier" });
        receivedObjects.Count.Should().Be(2);
    }

    [Fact]
    public async Task Run_WhenCharacterWasFoundInFormerNames_ShouldSendEventMergeTwoCharactersEvent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChangeNameDetectorService>>();
        var busPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<INameDetectorValidator>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeederVariablesSection>>();


        var charactersBeforeDetector = dbContext.Characters.AsNoTracking().ToList();

        for (var i = 0; i < charactersBeforeDetector.Count; i++)
        {
            SetupTibiaDataServiceMock(charactersBeforeDetector[i].Name, (i < 1, () => PrepareChangedNameCharacter(charactersBeforeDetector[i].Name)));
        }

        var changeNameDetector = new ChangeNameDetectorService(logger, validator, dbContext, _tibiaDataClientMock.Object, busPublisher, options);


        // Act
        await changeNameDetector.Run();


        // Assert
        var receivedObjects = SubscribeRabbitMessagesFromQueue<MergeTwoCharactersEvent>();

        var charactersAfterDetector = dbContext.Characters.AsNoTracking().ToList();


        charactersAfterDetector.Select(c => c.VerifiedDate).Should().AllBeEquivalentTo(DateOnly.FromDateTime(DateTime.Now));
        charactersAfterDetector.Count.Should().Be(4);
        charactersBeforeDetector.Select(c => c.VerifiedDate).Should().OnlyContain(date => date == null);

        receivedObjects[0].OldCharacterName.Should().Be("aphov");
        receivedObjects[0].NewCharacterName.Should().Be("asiier");
        receivedObjects.Count.Should().Be(1);
    }

    [Fact]
    public async Task Run_WhenCharacterWasFoundInFormerNamesButNotFoundInDatabase_ShouldChangeOldNameToNewName()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ChangeNameDetectorService>>();
        var busPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ITibiaStalkerDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<INameDetectorValidator>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<SeederVariablesSection>>();


        var charactersBeforeDetector = dbContext.Characters.AsNoTracking().ToList();

        for (var i = 0; i < charactersBeforeDetector.Count; i++)
        {
            SetupTibiaDataServiceMock(charactersBeforeDetector[i].Name,
                (i < 1, () => PrepareChangedNameCharacterWithNameNonExistentInDatabase(charactersBeforeDetector[i].Name)));
        }

        var changeNameDetector = new ChangeNameDetectorService(logger, validator, dbContext, _tibiaDataClientMock.Object, busPublisher, options);


        // Act
        await changeNameDetector.Run();


        // Assert
        var receivedObjects = SubscribeRabbitMessagesFromQueue<MergeTwoCharactersEvent>();

        var charactersAfterDetector = dbContext.Characters.AsNoTracking().ToList();


        charactersAfterDetector.Select(c => c.VerifiedDate).Should().AllBeEquivalentTo(DateOnly.FromDateTime(DateTime.Now));
        charactersAfterDetector.Count.Should().Be(4);
        charactersBeforeDetector.Select(c => c.VerifiedDate).Should().OnlyContain(date => date == null);
        charactersAfterDetector.Select(c => c.Name).Should().Contain(name => name == "test");
        charactersAfterDetector.Select(c => c.Name).Should().NotContain(name => name == charactersBeforeDetector.Select(c => c.Name).First());

        receivedObjects.Count.Should().Be(0);
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var initialization = scope.ServiceProvider.GetRequiredService<InitializationRabbitMqTaskRunner>();
        await initialization.StartAsync();
    }

    public Task DisposeAsync() => _resetDatabase();

    private List<T> SubscribeRabbitMessagesFromQueue<T>() where T: IntegrationEvent
    {
        var receivedObjects = new List<T>();

        var options = GetOptions();
        var factory = GetFactory(options);

        using var connection = factory.CreateConnection("tibia-stalker-subscriber");
        using var channel = connection.CreateModel();
        var exchangeOptions = options.Exchange;
        var deadLetterOptions = options.DeadLetter;
        var queueOptions = options.Queue;

        var queueName = $"{exchangeOptions.Name}.{typeof(T).Name.ToRabbitSnakeCase()}";

        Dictionary<string, object> queueArguments = new()
        {
            { "x-dead-letter-exchange", $"{deadLetterOptions.Prefix}{exchangeOptions.Name}" },
            { "x-dead-letter-routing-key", $"{deadLetterOptions.Prefix}{queueName}" }
        };

        channel.QueueDeclare(queue: queueName, queueOptions.Durable, queueOptions.Exclusive, queueOptions.AutoDelete, queueArguments);

        while (true)
        {
            BasicGetResult result = channel.BasicGet(queueName, autoAck: true);

            if (result == null)
                break;

            byte[] receivedBytes = result.Body.ToArray();
            string receivedMessage = Encoding.UTF8.GetString(receivedBytes);
            receivedObjects.Add(JsonConvert.DeserializeObject<T>(receivedMessage)!);
        }

        channel.Close();
        connection.Close();

        return receivedObjects;
    }

    private RabbitMqSection GetOptions()
    {
        var configuration = _factory.Configuration;

        var section = configuration.GetSection(RabbitMqSection.SectionName);

        var options = section.Get<RabbitMqSection>();

        return options!;
    }

    private ConnectionFactory GetFactory(RabbitMqSection options)
    {
        return new ConnectionFactory
        {
            Uri = new Uri(options!.HostUrl),
            Port = options.Port,
            VirtualHost = options.VirtualHost,
            UserName = options.Username,
            Password = options.Password,
            DispatchConsumersAsync = true
        };
    }

    private void SetupTibiaDataServiceMock(string characterName,
        params (bool Flag, Func<CharacterResult> PrepareFunction)[] preparations)
    {
        var preparation = preparations
            .FirstOrDefault(p => p.Flag);

        var result = preparation.Flag ? preparation.PrepareFunction.Invoke() : PrepareExistingTibiaDataCharacter(characterName);

        _tibiaDataClientMock
            .Setup(r => r.FetchCharacter(characterName, false))
            .ReturnsAsync(result);
    }

    private CharacterResult PrepareExistingTibiaDataCharacter(string name)
    {
        return new CharacterResult()
        {
            Name = name,
            Level = 100,
            Vocation = "Druid",
            World = "Adra",
            LastLogin = "2020-08-31T13:47:00Z",
            Traded = false
        };
    }

    private CharacterResult PrepareNonExistingTibiaDataCharacter()
    {
        return new CharacterResult()
        {
            Name = "",
            Level = 0,
            Vocation = "",
            World = "",
            LastLogin = "",
            Traded = false
        };
    }

    private CharacterResult PrepareTradedTibiaDataCharacter(string name)
    {
        return new CharacterResult()
        {
            Name = name,
            Level = 100,
            Vocation = "Druid",
            World = "Adra",
            LastLogin = "2020-08-31T13:47:00Z",
            Traded = true
        };
    }

    private CharacterResult PrepareChangedNameCharacter(string name)
    {
        return new CharacterResult()
        {
            Name = "asiier",
            FormerNames = new[] { name, "test2" },
            Level = 100,
            Vocation = "Druid",
            World = "Adra",
            LastLogin = "2020-08-31T13:47:00Z",
            Traded = false
        };
    }

    private CharacterResult PrepareChangedNameCharacterWithNameNonExistentInDatabase(string name)
    {
        return new CharacterResult()
        {
            Name = "test",
            FormerNames = new[] { name, "test2" },
            Level = 100,
            Vocation = "Druid",
            World = "Adra",
            LastLogin = "2020-08-31T13:47:00Z",
            Traded = false
        };
    }
}