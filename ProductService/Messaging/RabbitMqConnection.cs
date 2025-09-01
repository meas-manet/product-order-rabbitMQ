using RabbitMQ.Client;

namespace ProductService.Messaging;

public interface IRabbitMqConnection
{
    Task<IConnection> GetConnectionAsync();
}

public sealed class RabbitMqConnection : IRabbitMqConnection, IAsyncDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;

    public RabbitMqConnection(IConfiguration config)
    {
        _factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(config["RabbitMq:Port"] ?? "5672"),
            UserName = config["RabbitMq:Username"] ?? "guest",
            Password = config["RabbitMq:Password"] ?? "guest",
            ClientProvidedName = "product-service-conn"
        };
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true }) return _connection;
        _connection = await _factory.CreateConnectionAsync();
        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null) await _connection.CloseAsync();
        _connection?.Dispose();
    }
}
