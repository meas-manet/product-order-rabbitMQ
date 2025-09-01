using Microsoft.AspNetCore.Mvc;
using ProductService.Messaging;
using Shared.Contracts;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IDictionary<string, (string Name, decimal Price)> _db;
    private readonly IRabbitMqPublisher _publisher;

    public ProductsController(
        IDictionary<string, (string, decimal)> db,
        IRabbitMqPublisher publisher)
    { _db = db; _publisher = publisher; }

    [HttpPost]
    public async Task<ActionResult<CreateProductResponse>> Create(CreateProductRequest request)
    {
        var id = Guid.NewGuid().ToString("N");
        _db[id] = (request.Name, request.Price);

        var evt = new ProductCreated(id, request.Name, request.Price, DateTimeOffset.UtcNow);
        await _publisher.PublishAsync(RabbitSetup.ProductExchange, RabbitSetup.ProductCreatedRoutingKey, evt,
            p => p.Headers = new Dictionary<string, object> { ["x-origin"] = "ProductService" });

        return CreatedAtAction(nameof(GetById), new { id }, new CreateProductResponse(id, request.Name, request.Price));
    }

    [HttpGet("{id}")]
    public ActionResult<CreateProductResponse> GetById(string id)
    {
        if (!_db.TryGetValue(id, out var p)) return NotFound();
        return new CreateProductResponse(id, p.Name, p.Price);
    }
}
