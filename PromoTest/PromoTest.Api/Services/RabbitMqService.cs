using System.Text.Json;
using System.Threading.Channels;
using PromoTest.Api.Services.Interfaces;
using PromoTest.Api.Services.Messaging;
using RabbitMQ.Client;

namespace PromoTest.Api.Services;

public class RabbitMqService(
    IConfiguration configuration,
    ILogger<RabbitMqService> logger)
    : BackgroundService, ITaskEventPublisher
{
    private const string Exchange = "task.events";
    private const string RoutingKey = "task.completed";

    private readonly Channel<TaskCompletedEvent> _queue =
        Channel.CreateUnbounded<TaskCompletedEvent>();

    private IConnection? _connection;
    private IChannel? _channel;

    public ValueTask EnqueueAsync(TaskCompletedEvent message)
    {
        if (_queue.Writer.TryWrite(message))
            return ValueTask.CompletedTask;

        throw new InvalidOperationException("The RabbitMQ publishing queue is closed.");
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:Host"]!,
                Port = configuration.GetValue("RabbitMq:Port", 5672),
                UserName = configuration["RabbitMq:Username"]!,
                Password = configuration["RabbitMq:Password"]!
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);

            _channel = await _connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

            await _channel.ExchangeDeclareAsync(
                exchange: Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RabbitMQ initialization failed.");
        }

        await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            if (_channel is null)
            {
                logger.LogError("RabbitMQ unavailable. Event for task {TaskId} was not published.", message.Id);

                continue;
            }

            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(new
                {
                    taskId = message.Id,
                    title = message.Title,
                    completedAt = message.CompletedAt,
                    priority = message.Priority
                });

                await _channel.BasicPublishAsync(
                    exchange: Exchange,
                    routingKey: RoutingKey,
                    mandatory: false,
                    basicProperties: new BasicProperties
                    {
                        Persistent = true,
                        ContentType = "application/json"
                    },
                    body: body, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish completion event for task {TaskId}", message.Id);
            }
        }

        if (_channel is not null)
            await _channel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _queue.Writer.TryComplete();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeout.CancelAfter(TimeSpan.FromSeconds(4));

        try
        {
            await base.StopAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("RabbitMQ shutdown timed out; some events may not have been published.");
        }
    }
}