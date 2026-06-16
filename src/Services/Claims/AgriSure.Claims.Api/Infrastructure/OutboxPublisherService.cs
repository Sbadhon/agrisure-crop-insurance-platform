using System.Text.Json;
using AgriSure.BuildingBlocks.Messaging;
using AgriSure.Claims.Api.Data;
using AgriSure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AgriSure.Claims.Api.Infrastructure;

public sealed class OutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    RabbitMqPublisher publisher,
    ILogger<OutboxPublisherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(2));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Outbox publish cycle failed; messages will be retried.");
            }

            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task PublishBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null && x.Attempts < 20)
            .OrderBy(x => x.OccurredAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var envelope = JsonSerializer.Deserialize<IntegrationEventEnvelope>(message.Payload)
                    ?? throw new InvalidOperationException("Outbox payload could not be deserialized.");

                await publisher.PublishAsync(envelope, message.RoutingKey, cancellationToken);
                message.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception exception)
            {
                message.RecordFailure(exception.Message);
                logger.LogWarning(
                    exception,
                    "Failed to publish outbox message {MessageId}; attempt {Attempt}",
                    message.Id,
                    message.Attempts);
            }
        }

        if (messages.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
