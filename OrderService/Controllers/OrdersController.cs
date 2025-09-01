using Microsoft.AspNetCore.Mvc;
using OrderService.Messaging;
using Shared.Contracts;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IDictionary<string, (string ProductId, int Qty, string Status)> _db;
    private readonly IDictionary<string, (string Name, decimal Price)> _productCache;
    private readonly IRabbitMqPublisher _publisher;

    public OrdersController(
        IDictionary<string, (string ProductId, int Qty, string Status)> db,
        IDictionary<string, (string Name, decimal Price)> productCache,
        IRabbitMqPublisher publisher)
    { _db = db; _productCache = productCache; _publisher = publisher; }

    [HttpPost]
    public async Task<ActionResult<CreateOrderResponse>> Create(CreateOrderRequest request)
    {
        if (!_productCache.ContainsKey(request.ProductId))
            return BadRequest($"Unknown product id {request.ProductId}. Create the product first.");

        var id = Guid.NewGuid().ToString("N");
        _db[id] = (request.ProductId, request.Quantity, "Created");

        var evt = new OrderCreated(id, request.ProductId, request.Quantity, DateTimeOffset.UtcNow);
        await _publisher.PublishAsync(RabbitSetup.OrderExchange, RabbitSetup.OrderCreatedRoutingKey, evt);

        return CreatedAtAction(nameof(GetById), new { id },
            new CreateOrderResponse(id, request.ProductId, request.Quantity, "Created"));
    }

    [HttpGet("{id}")]
    public ActionResult<CreateOrderResponse> GetById(string id)
    {
        if (!_db.TryGetValue(id, out var o)) return NotFound();
        return new CreateOrderResponse(id, o.ProductId, o.Qty, o.Status);
    }
}
