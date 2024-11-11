﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.RabbitMQ.Configuration;

namespace ChangeNameDetectorSubscriber.Subscribers;

public class ChangeNameDetectorSubscriber : IChangeNameDetectorSubscriber
{
    private readonly IEnumerable<IEventSubscriber> _subscribers;
    private readonly ILogger<ChangeNameDetectorSubscriber> _logger;
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqSection _options;
    private const string RetryHeaderName = "x-redelivered-count";

    private readonly List<IModel> _channels = new ();

    public ChangeNameDetectorSubscriber(IEnumerable<IEventSubscriber> subscribers,
        ILogger<ChangeNameDetectorSubscriber> logger,
        RabbitMqConnection connection,
        IOptions<RabbitMqSection> options)
    {
        _subscribers = subscribers;
        _logger = logger;
        _connection = connection;
        _options = options.Value;
    }

    public void Subscribe()
    {
        foreach (var eventSubscriber in _subscribers)
        {
            RegisterConsumer(eventSubscriber);
        }
    }

    public void CloseChannels()
    {
        foreach (var channel in _channels)
        {
            channel.Close();
        }
    }

    private void RegisterConsumer(IEventSubscriber eventSubscriber)
    {
        IModel  channel = _connection.Connection.CreateModel();
        _channels.Add(channel);

        channel.BasicQos(0, 1, true);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, deliveryArguments) =>
        {
            var retryCount = GetRetryCount(deliveryArguments.BasicProperties);

            if (retryCount >= _options.Retries)
            {
                channel.BasicReject(deliveryArguments.DeliveryTag, false);
                return;
            }

            try
            {
                await eventSubscriber.OnReceived(deliveryArguments);
                channel.BasicAck(deliveryTag: deliveryArguments.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on received message: {Message}", ex.Message);
                channel.BasicReject(deliveryArguments.DeliveryTag, false);
            }
        };

        try
        {
            channel.BasicConsume(queue: eventSubscriber.GetQueueName(), autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error on register consumer {Consumer}: {Message}", eventSubscriber.GetType(), ex.Message);
        }
    }

    private static int GetRetryCount(IBasicProperties messageProperties)
    {
         var result = messageProperties.Headers is not null && messageProperties.Headers.ContainsKey(RetryHeaderName)
            ? (int)messageProperties.Headers[RetryHeaderName] : 0;

         return result;
    }
}