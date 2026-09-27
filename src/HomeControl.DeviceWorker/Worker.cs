using System.Text.Json;
using HomeControl.Application.Features.Devices.IntegrationEvents;
using HomeControl.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HomeControl.DeviceWorker;

public sealed class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = RabbitMqSettings.FromConfiguration(configuration);
        await using var connection = await settings.CreateConnectionFactory()
            .CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            RabbitMqTopology.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(
            RabbitMqTopology.AuditQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);
        await channel.QueueBindAsync(
            RabbitMqTopology.AuditQueue,
            RabbitMqTopology.Exchange,
            RabbitMqTopology.AuditBinding,
            cancellationToken: stoppingToken);
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: stoppingToken);

        var processingFailure = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<DeviceLightStateChanged>(delivery.Body.Span);
                if (message is null || message.DeviceId == Guid.Empty || message.OccurredAtUtc == default)
                {
                    throw new JsonException("Invalid device light state message.");
                }

                logger.LogInformation(
                    "Device audit: DeviceId={DeviceId}, IsOn={IsOn}, OccurredAtUtc={OccurredAtUtc}, RoutingKey={RoutingKey}",
                    message.DeviceId,
                    message.IsOn,
                    message.OccurredAtUtc,
                    delivery.RoutingKey);

                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Failed to process device audit message with routing key {RoutingKey}; message was not acknowledged.",
                    delivery.RoutingKey);
                processingFailure.TrySetException(exception);
            }
        };

        await channel.BasicConsumeAsync(RabbitMqTopology.AuditQueue, autoAck: false, consumer,
            cancellationToken: stoppingToken);
        logger.LogInformation("Consuming {Queue} with binding {Binding}",
            RabbitMqTopology.AuditQueue, RabbitMqTopology.AuditBinding);

        await processingFailure.Task.WaitAsync(stoppingToken);
    }
}
