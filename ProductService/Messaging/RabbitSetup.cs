using RabbitMQ.Client;

namespace ProductService.Messaging;

public static class RabbitSetup
{
    public const string ProductExchange = "products.v1";
    public const string ProductCreatedRoutingKey = "product.created";

    public static async Task EnsureTopologyAsync(IRabbitMqConnection conn)
    {
        var connection = await conn.GetConnectionAsync();
        await using var ch = await connection.CreateChannelAsync();

        await ch.ExchangeDeclareAsync(ProductExchange, ExchangeType.Topic, durable: true);
        await ch.QueueDeclareAsync("product.created.queue", durable: true, exclusive: false, autoDelete: false);
        await ch.QueueBindAsync("product.created.queue", ProductExchange, ProductCreatedRoutingKey);
    }
}
