using System.Text.Json;
using AgriSure.BuildingBlocks.Messaging;
using AgriSure.Contracts;
using AgriSure.Operations.Api.Data;
using AgriSure.Operations.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgriSure.Operations.Api.Messaging;

public sealed class ClaimEventConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<ClaimEventConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumerAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "RabbitMQ consumer disconnected; retrying in 5 seconds.");
                await DisposeBrokerResourcesAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task StartConsumerAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(_options.Uri),
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        const string deadLetterExchange = "agrisure.dead-letter";
        const string deadLetterQueue = "agrisure.operations.dead-letter";
        const string queueName = "agrisure.operations";

        await _channel.ExchangeDeclareAsync(
            _options.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(
            deadLetterExchange,
            ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            deadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            deadLetterQueue,
            deadLetterExchange,
            string.Empty,
            arguments: null,
            cancellationToken: cancellationToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = deadLetterExchange
        };
        await _channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            queueName,
            _options.Exchange,
            "claims.#",
            arguments: null,
            cancellationToken: cancellationToken);
        await _channel.BasicQosAsync(0, 20, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;
        await _channel.BasicConsumeAsync(
            queueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation("Operations consumer is listening on {QueueName}", queueName);
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(args.Body.Span)
                ?? throw new InvalidOperationException("Event envelope was empty.");
            var payload = envelope.Payload.Deserialize<ClaimEventPayload>()
                ?? throw new InvalidOperationException("Claim event payload was empty.");

            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<OperationsDbContext>();

            if (await db.ProcessedMessages.AnyAsync(
                x => x.EventId == envelope.EventId,
                args.CancellationToken))
            {
                await _channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: args.CancellationToken);
                return;
            }

            var projection = await db.Claims.SingleOrDefaultAsync(
                x => x.ClaimId == payload.ClaimId,
                args.CancellationToken);

            if (projection is null)
            {
                projection = new ClaimProjection(
                    payload.ClaimId,
                    envelope.TenantId,
                    payload.PolicyId,
                    payload.ClaimNumber,
                    payload.PolicyNumber,
                    payload.ProducerName,
                    payload.Crop,
                    payload.County,
                    payload.Status,
                    payload.EstimatedIndemnity,
                    payload.Note,
                    payload.UpdatedAtUtc);
                db.Claims.Add(projection);
            }
            else
            {
                projection.Apply(
                    payload.Status,
                    payload.EstimatedIndemnity,
                    payload.Note,
                    payload.UpdatedAtUtc);
            }

            db.ProcessedMessages.Add(new ProcessedMessage(
                envelope.EventId,
                envelope.EventType,
                DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(args.CancellationToken);
            await _channel.BasicAckAsync(
                args.DeliveryTag,
                multiple: false,
                cancellationToken: args.CancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process event with delivery tag {DeliveryTag}", args.DeliveryTag);
            await _channel.BasicNackAsync(
                args.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: args.CancellationToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeBrokerResourcesAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task DisposeBrokerResourcesAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
