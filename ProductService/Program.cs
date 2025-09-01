using ProductService.Messaging;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
builder.Services.AddSingleton<IDictionary<string, (string Name, decimal Price)>>(new Dictionary<string, (string, decimal)>());

var app = builder.Build();

var conn = app.Services.GetRequiredService<IRabbitMqConnection>();
await RabbitSetup.EnsureTopologyAsync(conn);

app.MapControllers();
app.Run();
