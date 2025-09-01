using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OrderService.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishAsync(string exchange, string routingKey, object message);
}

public sealed class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly IRabbitMqConnection _conn;
    public RabbitMqPublisher(IRabbitMqConnection conn) => _conn = conn;

    public async Task PublishAsync(string exchange, string routingKey, object message)
    {
        var connection = await _conn.GetConnectionAsync();
        await using var ch = await connection.CreateChannelAsync();

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = (DeliveryModes)2,
            MessageId = Guid.NewGuid().ToString("N"),
            CorrelationId = Guid.NewGuid().ToString("N"),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await ch.BasicPublishAsync(exchange, routingKey, false, props, body);
    }
}
