using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts;

namespace OrderService.Messaging;

public sealed class ProductCreatedConsumer : BackgroundService
{
    private readonly IRabbitMqConnection _conn;
    private readonly ILogger<ProductCreatedConsumer> _logger;
    private readonly IDictionary<string, (string Name, decimal Price)> _products;
    private IChannel? _ch;

    public ProductCreatedConsumer(
        IRabbitMqConnection conn,
        ILogger<ProductCreatedConsumer> logger,
        IDictionary<string, (string, decimal)> products)
    {
        _conn = conn; _logger = logger; _products = products;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await _conn.GetConnectionAsync();
        _ch = await connection.CreateChannelAsync();
        await _ch.BasicQosAsync(0, 10, false);

        var consumer = new AsyncEventingBasicConsumer(_ch);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<ProductCreated>(json)!;

                _products[evt.ProductId] = (evt.Name, evt.Price);
                _logger.LogInformation("Cached product {Id} - {Name}", evt.ProductId, evt.Name);

                await _ch!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process ProductCreated");
                await _ch!.BasicNackAsync(ea.DeliveryTag, false, requeue: true);
            }
        };

        await _ch.BasicConsumeAsync(RabbitSetup.ProductEventsQueue, autoAck: false, consumer);
    }

    public override void Dispose()
    {
        _ch?.Dispose();
        base.Dispose();
    }
}
