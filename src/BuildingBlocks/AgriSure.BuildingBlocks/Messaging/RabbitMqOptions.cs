namespace AgriSure.BuildingBlocks.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Uri { get; init; } = "amqp://agrisure:agrisure-dev@localhost:5672";
    public string Exchange { get; init; } = "agrisure.events";
}
