# RabbitMqDemo (.NET 9 + RabbitMQ)

A minimal event-driven demo with two independent microservices communicating via RabbitMQ.

- **ProductService**

  - `POST /api/products` → creates a product and publishes a `product.created` event
  - `GET /api/products/{id}` → fetch a product by id

- **OrderService**
  - Listens for `product.created` events from RabbitMQ and caches products locally
  - `POST /api/orders` → creates an order if the product exists in the cache, publishes `order.created`
  - `GET /api/orders/{id}` → fetch an order by id

Both services use the official [`RabbitMQ.Client`](https://www.nuget.org/packages/RabbitMQ.Client) **7.x** client library (async APIs, `IChannel`, `BasicPublishAsync`, etc.).

---

## 🛠 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (for RabbitMQ)
- `curl`, Postman, or any HTTP client for testing

---

## 🚀 Getting Started

### 2. Start RabbitMQ

```bash
docker compose up -d
```

### 3. Run ProductService

```bash
dotnet run --project ProductService/ProductService.csproj --urls http://localhost:5080
```

### 4. Run OrderService

```bash
dotnet run --project OrderService/OrderService.csproj --urls http://localhost:5090
```

## 📡 Usage

### Create a Product

```bash
curl -s -X POST http://localhost:5080/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Road Bike","price":1299.99}'
```

### Fetch Product

```bash
curl http://localhost:5080/api/products/<PRODUCT_ID>
```

### Create an Order

```bash
curl http://localhost:5090/api/orders/<ORDER_ID>
```
