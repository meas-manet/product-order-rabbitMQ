using RabbitMQ.Client;

namespace OrderService.Messaging;

public static class RabbitSetup
{
    public const string OrderExchange = "orders.v1";
    public const string OrderCreatedRoutingKey = "order.created";

    public const string ProductExchange = "products.v1";
    public const string ProductCreatedRoutingKey = "product.created";
    public const string ProductEventsQueue = "order-service.product.created.queue";

    public static async Task EnsureTopologyAsync(IRabbitMqConnection conn)
    {
        var connection = await conn.GetConnectionAsync();
        await using var ch = await connection.CreateChannelAsync();

        await ch.ExchangeDeclareAsync(OrderExchange, ExchangeType.Topic, durable: true);

        await ch.ExchangeDeclareAsync(ProductExchange, ExchangeType.Topic, durable: true);
        await ch.QueueDeclareAsync(ProductEventsQueue, durable: true, exclusive: false, autoDelete: false);
        await ch.QueueBindAsync(ProductEventsQueue, ProductExchange, ProductCreatedRoutingKey);

        // Optional DLX if you want it later:
        await ch.ExchangeDeclareAsync("dlx", ExchangeType.Topic, durable: true);
    }
}
