using OrderService.Messaging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddSingleton<IDictionary<string, (string ProductId, int Qty, string Status)>>(
    new Dictionary<string, (string, int, string)>());

builder.Services.AddSingleton<IDictionary<string, (string Name, decimal Price)>>(
    new Dictionary<string, (string, decimal)>());

builder.Services.AddHostedService<ProductCreatedConsumer>();

var app = builder.Build();

var conn = app.Services.GetRequiredService<IRabbitMqConnection>();
await RabbitSetup.EnsureTopologyAsync(conn);

app.MapControllers();
app.Run();
