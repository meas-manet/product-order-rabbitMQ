namespace Shared.Contracts;

// Events
public record ProductCreated(string ProductId, string Name, decimal Price, DateTimeOffset CreatedAt);
public record OrderCreated(string OrderId, string ProductId, int Quantity, DateTimeOffset CreatedAt);

// API DTOs
public record CreateProductRequest(string Name, decimal Price);
public record CreateProductResponse(string Id, string Name, decimal Price);

public record CreateOrderRequest(string ProductId, int Quantity);
public record CreateOrderResponse(string Id, string ProductId, int Quantity, string Status);
