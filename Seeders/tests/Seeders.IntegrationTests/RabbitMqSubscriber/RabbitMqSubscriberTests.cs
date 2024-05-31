﻿using System.Text;
using ChangeNameDetectorSubscriber.Subscribers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.RabbitMQ.Configuration;
using Shared.RabbitMq.Conventions;
using Shared.RabbitMQ.Conventions;
using Shared.RabbitMQ.Events;
using Shared.RabbitMQ.Initializers;
using TibiaStalker.Domain.Entities;
using TibiaStalker.Infrastructure.Persistence;

namespace Seeders.IntegrationTests.RabbitMqSubscriber;

[Collection("Seeder test collection")]
public class RabbitMqSubscriberTests : IAsyncLifetime
{
    private readonly TibiaSeederFactory _factory;
    private readonly Func<Task> _resetDatabase;
    private readonly MessageSerializer _serializer = new();

    public RabbitMqSubscriberTests(TibiaSeederFactory factory)
    {
        _factory = factory;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    [Fact]
    public async Task Subscribe_WhenCharacterNotExist_ShouldDeleteCharacterWithCorrelations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TibiaSubscriber>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqSection>>();

        var publisherConnection = GetRabbitMqConnection(options, "publisher");
        var subscriberConnection = GetRabbitMqConnection(options, "subscriber");

        await _factory.ClearDatabaseAsync(dbContext);
        await SeedDatabaseAsync(dbContext);

        var subscribers = scope.ServiceProvider.GetServices<IEventSubscriber>();
        var message = new DeleteCharacterWithCorrelationsEvent("asiier");
        var tibiaSubscriber = new TibiaSubscriber(subscribers, logger, subscriberConnection, options);

        PublishRabbitMessagesToQueue(options, publisherConnection, message);

        // Act
        tibiaSubscriber.Subscribe();
        Thread.Sleep(500);

        publisherConnection.Connection.Close();
        subscriberConnection.Connection.Close();


        // Assert
        var dbContextAfterSubscribe = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();
        var charactersAfterSubscriber = dbContextAfterSubscribe.Characters.AsNoTracking().ToList();
        var characterCorrelationsAfterSubscriber = dbContextAfterSubscribe.CharacterCorrelations.AsNoTracking().ToList();

        charactersAfterSubscriber.Count.Should().Be(3);
        characterCorrelationsAfterSubscriber.Count.Should().Be(3);
    }

    [Fact]
    public async Task Subscribe_WhenCharacterWasTraded_ShouldDeleteCorrelations()
    {
        // Arrange
        // TODO: get out initDateOnly to public static class
        var initDateOnly = new DateOnly(2001, 01, 01);
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TibiaSubscriber>>();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqSection>>();

        var publisherConnection = GetRabbitMqConnection(options, "publisher");
        var subscriberConnection = GetRabbitMqConnection(options, "subscriber");

        var dbContext = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();

        await _factory.ClearDatabaseAsync(dbContext);
        await SeedDatabaseAsync(dbContext);

        var subscribers = scope.ServiceProvider.GetServices<IEventSubscriber>();
        var message = new DeleteCharacterCorrelationsEvent("asiier");
        var tibiaSubscriber = new TibiaSubscriber(subscribers, logger, subscriberConnection, options);

        PublishRabbitMessagesToQueue(options, publisherConnection, message);

        // Act
        tibiaSubscriber.Subscribe();
        Thread.Sleep(500);

        publisherConnection.Connection.Close();
        subscriberConnection.Connection.Close();


        // Assert
        var dbContextAfterSubscribe = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();
        var charactersAfterSubscriber = dbContextAfterSubscribe.Characters.AsNoTracking().ToList();
        var characterCorrelationsAfterSubscriber =
            dbContextAfterSubscribe.CharacterCorrelations.AsNoTracking().ToList();

        charactersAfterSubscriber.Count.Should().Be(4);
        charactersAfterSubscriber.Count(c => c.TradedDate != initDateOnly).Should().Be(1);
        characterCorrelationsAfterSubscriber.Count.Should().Be(3);
    }

    [Fact]
    public async Task Subscribe_WhenCharacterWasFoundInFormerNames_ShouldMergeProperCorrelations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TibiaSubscriber>>();

        var options = scope.ServiceProvider.GetRequiredService<IOptions<RabbitMqSection>>();

        var publisherConnection = GetRabbitMqConnection(options, "publisher");
        var subscriberConnection = GetRabbitMqConnection(options, "subscriber");

        var dbContext = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();

        await _factory.ClearDatabaseAsync(dbContext);
        await SeedDatabaseAsync(dbContext);

        var subscribers = scope.ServiceProvider.GetServices<IEventSubscriber>();
        var message = new MergeTwoCharactersEvent("aphov", "asiier");
        var tibiaSubscriber = new TibiaSubscriber(subscribers, logger, subscriberConnection, options);

        PublishRabbitMessagesToQueue(options, publisherConnection, message);

        // Act
        tibiaSubscriber.Subscribe();
        Thread.Sleep(500);

        publisherConnection.Connection.Close();
        subscriberConnection.Connection.Close();

        // Assert
        var dbContextAfterSubscribe = scope.ServiceProvider.GetRequiredService<TibiaStalkerDbContext>();
        var charactersAfterSubscriber = dbContextAfterSubscribe.Characters.AsNoTracking().ToList();
        var characterCorrelationsAfterSubscriber = dbContextAfterSubscribe.CharacterCorrelations.AsNoTracking().ToList();

        charactersAfterSubscriber.Count.Should().Be(3);
        characterCorrelationsAfterSubscriber.Count.Should().Be(3);
        characterCorrelationsAfterSubscriber.First(cc => cc is { LoginCharacterId: 123, LogoutCharacterId: 121 }).NumberOfMatches.Should().Be(6);
        characterCorrelationsAfterSubscriber.First(cc => cc is { LoginCharacterId: 122, LogoutCharacterId: 121 }).NumberOfMatches.Should().Be(9);
        characterCorrelationsAfterSubscriber.First(cc => cc is { LoginCharacterId: 123, LogoutCharacterId: 122 }).NumberOfMatches.Should().Be(5);
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var initialization = scope.ServiceProvider.GetRequiredService<InitializationRabbitMqTaskRunner>();
        await initialization.StartAsync();
    }

    public Task DisposeAsync() => _resetDatabase();

    private void PublishRabbitMessagesToQueue(IOptions<RabbitMqSection> optionsSection, RabbitMqConnection connection, object message)
    {
        var options = optionsSection.Value;

        IModel channel = connection.Connection.CreateModel();
        var exchangeOptions = options.Exchange;
        var deadLetterOptions = options.DeadLetter;
        var queueOptions = options.Queue;

        var routingKey = message.GetType().Name.ToRabbitSnakeCase();
        var queueName = $"{exchangeOptions.Name}.{routingKey}";

        Dictionary<string, object> queueArguments = new()
        {
            { "x-dead-letter-exchange", $"{deadLetterOptions.Prefix}{exchangeOptions.Name}" },
            { "x-dead-letter-routing-key", $"{deadLetterOptions.Prefix}{queueName}" }
        };

        channel.QueueDeclare(queue: queueName, queueOptions.Durable, queueOptions.Exclusive, queueOptions.AutoDelete, queueArguments);

        var serializedMessage = _serializer.Serialize(message);
        byte[] body = Encoding.UTF8.GetBytes(serializedMessage);

        channel.BasicPublish(exchange: exchangeOptions.Name, routingKey: routingKey, basicProperties: null, body: body);
    }

    private RabbitMqConnection GetRabbitMqConnection(IOptions<RabbitMqSection> optionsSection, string conectionName)
    {
        var options = optionsSection.Value;

        var factory = new ConnectionFactory
        {
            Uri = new Uri(options!.HostUrl),
            Port = options.Port,
            VirtualHost = options.VirtualHost,
            UserName = options.Username,
            Password = options.Password,
            DispatchConsumersAsync = true
        };

        IConnection connection = factory.CreateConnection(conectionName);

        return new RabbitMqConnection(connection);
    }

    private async Task SeedDatabaseAsync(TibiaStalkerDbContext dbContext)
    {
        await dbContext.Worlds.AddRangeAsync(GetWorlds());
        await dbContext.Characters.AddRangeAsync(GetCharacters());
        await dbContext.CharacterCorrelations.AddRangeAsync(GetCharacterCorrelations());

        await dbContext.SaveChangesAsync();
    }

    private IEnumerable<World> GetWorlds()
    {
        return new List<World>
        {
            new() { WorldId = 11, Name = "Damora", IsAvailable = true, Url = "https://www.tibia.com/community/?subtopic=worlds&world=Damora" },
        };
    }

    private IEnumerable<Character> GetCharacters()
    {
        return new List<Character>
        {
            new() {CharacterId = 120, WorldId = 11, Name = "aphov"},
            new() {CharacterId = 121, WorldId = 11, Name = "asiier"},
            new() {CharacterId = 122, WorldId = 11, Name = "armystrong"},
            new() {CharacterId = 123, WorldId = 11, Name = "brytiaggo"},
        };
    }

    private IEnumerable<CharacterCorrelation> GetCharacterCorrelations()
    {
        return new List<CharacterCorrelation>
        {
            new() { LoginCharacterId = 120, LogoutCharacterId = 123, NumberOfMatches = 2},
            new() { LoginCharacterId = 121, LogoutCharacterId = 122, NumberOfMatches = 3},
            new() { LoginCharacterId = 123, LogoutCharacterId = 121, NumberOfMatches = 4},
            new() { LoginCharacterId = 123, LogoutCharacterId = 122, NumberOfMatches = 5},
            new() { LoginCharacterId = 122, LogoutCharacterId = 120, NumberOfMatches = 6}
        };
    }
}