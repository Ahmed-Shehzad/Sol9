# Application Modules

Sol9 demonstrates a modular monolith architecture with two main application modules: Bookings and Orders.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Gateway.API                          │
│              (YARP Reverse Proxy)                       │
└──────────────┬──────────────────────┬───────────────────┘
               │                      │
    ┌──────────▼──────────┐  ┌───────▼──────────┐
    │   Bookings.API      │  │   Orders.API     │
    │  (HTTP + gRPC)      │  │  (HTTP + gRPC)   │
    └──────────┬──────────┘  └───────┬──────────┘
               │                      │
    ┌──────────▼──────────┐  ┌───────▼──────────┐
    │ Bookings.Application │  │ Orders.Application│
    │  (Domain Logic)      │  │  (Domain Logic)   │
    └──────────┬──────────┘  └───────┬──────────┘
               │                      │
    ┌──────────▼──────────┐  ┌───────▼──────────┐
    │ Bookings.Domain      │  │ Orders.Domain     │
    │  (Entities)          │  │  (Entities)       │
    └──────────┬──────────┘  └───────┬──────────┘
               │                      │
    ┌──────────▼──────────┐  ┌───────▼──────────┐
    │Bookings.Infrastructure│ │Orders.Infrastructure│
    │  (EF Core + Repos)   │  │  (EF Core + Repos)  │
    └──────────────────────┘  └─────────────────────┘
```

## Bookings Module

### Overview

The Bookings module manages booking creation and lifecycle.

### Structure

```
Bookings.API/              # Web API layer
Bookings.Application/       # Application logic (commands, queries, handlers)
Bookings.Domain/           # Domain entities and business rules
Bookings.Infrastructure/   # Data access (EF Core, repositories)
```

### Features

- **Create Booking**: Create bookings from orders
- **Cancel Booking**: Cancel existing bookings
- **Get Booking**: Retrieve booking details
- **Saga Orchestration**: Process booking creation with compensation

### API Endpoints

```
POST   /api/v1/bookings          # Create booking
GET    /api/v1/bookings/{id}      # Get booking
DELETE /api/v1/bookings/{id}      # Cancel booking
```

### Example Usage

```csharp
// Create booking via API
POST /api/v1/bookings
{
    "orderId": "01ARZ3NDEKTSV4RRFFQ69G5FAV",
    "customerName": "John Doe"
}

// Response
{
    "id": "01ARZ3NDEKTSV4RRFFQ69G5FAW",
    "orderId": "01ARZ3NDEKTSV4RRFFQ69G5FAV",
    "customerName": "John Doe",
    "status": "Created"
}
```

### Integration with Orders

Bookings receives requests from Orders via Transponder:

```csharp
// Orders.API sends request
var client = _clientFactory.CreateRequestClient<CreateBookingRequest>();
var response = await client.GetResponseAsync<CreateBookingResponse>(
    new CreateBookingRequest(order.Id, order.CustomerName));
```

## Orders Module

### Overview

The Orders module manages order creation, processing, and lifecycle.

### Structure

```
Orders.API/              # Web API layer
Orders.Application/       # Application logic (commands, queries, handlers)
Orders.Domain/           # Domain entities and business rules
Orders.Infrastructure/   # Data access (EF Core, repositories)
```

### Features

- **Create Order**: Create new orders
- **Get Order**: Retrieve order details
- **Get Orders**: List all orders
- **Order Processing**: Process orders with booking creation

### API Endpoints

```
POST   /api/v1/orders          # Create order
GET    /api/v1/orders/{id}      # Get order
GET    /api/v1/orders           # Get all orders
```

### Example Usage

```csharp
// Create order via API
POST /api/v1/orders
{
    "customerName": "John Doe",
    "totalAmount": 99.99
}

// Response
{
    "id": "01ARZ3NDEKTSV4RRFFQ69G5FAV",
    "customerName": "John Doe",
    "totalAmount": 99.99,
    "status": "Created"
}
```

### Integration with Bookings

Orders sends requests to Bookings via Transponder:

```csharp
// Orders.Application
public class CreateOrderCommandHandler
{
    private readonly IClientFactory _clientFactory;
    
    public async Task<OrderDto> HandleAsync(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerName, command.TotalAmount);
        await _repository.SaveAsync(order);
        
        // Request booking creation
        var client = _clientFactory.CreateRequestClient<CreateBookingRequest>();
        var bookingResponse = await client.GetResponseAsync<CreateBookingResponse>(
            new CreateBookingRequest(order.Id, order.CustomerName));
        
        return MapToDto(order);
    }
}
```

## Gateway.API

### Overview

Gateway.API is a YARP-based reverse proxy that routes requests to backend services.

### Routing

```
/bookings/*  →  Bookings.API
/orders/*    →  Orders.API
```

### Configuration

```csharp
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.UseLoadBalancing();
    proxyPipeline.UseSessionAffinity();
});
```

## Communication Patterns

### Request/Response

Orders → Bookings (synchronous):

```csharp
// Orders sends request
var response = await client.GetResponseAsync<CreateBookingResponse>(
    new CreateBookingRequest(order.Id, order.CustomerName));
```

### Publish/Subscribe

Order created event (asynchronous):

```csharp
// Orders publishes event
await _publisher.PublishAsync(new OrderCreatedEvent(order.Id));

// Multiple handlers can react
// - Send notification email
// - Update cache
// - Update analytics
```

### Saga Orchestration

Distributed transaction coordination:

```csharp
// Saga coordinates multiple steps
public class ProcessOrderSaga
{
    public async Task HandleAsync(OrderCreatedEvent evt)
    {
        // Step 1: Create booking
        await _bus.SendAsync(new CreateBookingCommand(evt.OrderId));
        
        // Step 2: Process payment
        await _bus.SendAsync(new ProcessPaymentCommand(evt.OrderId));
        
        // If any step fails, compensate previous steps
    }
}
```

## Database Schema

### Bookings Database

```sql
CREATE TABLE bookings (
    id UUID PRIMARY KEY,
    order_id UUID NOT NULL,
    customer_name VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL
);
```

### Orders Database

```sql
CREATE TABLE orders (
    id UUID PRIMARY KEY,
    customer_name VARCHAR(100) NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL
);
```

### Transponder Persistence

```sql
-- Outbox table
CREATE TABLE transponder_outbox (
    id UUID PRIMARY KEY,
    message_type VARCHAR(255) NOT NULL,
    destination_address VARCHAR(500) NOT NULL,
    body BYTEA NOT NULL,
    created_at TIMESTAMP NOT NULL
);

-- Inbox table
CREATE TABLE transponder_inbox (
    message_id UUID PRIMARY KEY,
    message_type VARCHAR(255) NOT NULL,
    processed_at TIMESTAMP NOT NULL
);

-- Saga state table
CREATE TABLE transponder_saga_state (
    correlation_id UUID PRIMARY KEY,
    conversation_id UUID,
    saga_type VARCHAR(255) NOT NULL,
    state_data JSONB NOT NULL,
    version INT NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);
```

## Testing

### Unit Tests

Test application logic in isolation:

```csharp
[Fact]
public async Task CreateOrder_Should_Save_Order()
{
    var handler = new CreateOrderCommandHandler(_repository);
    var command = new CreateOrderCommand("John Doe", 99.99);
    
    var result = await handler.HandleAsync(command);
    
    Assert.NotNull(result);
    _repository.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Once);
}
```

### Integration Tests

Test with real database (requires Docker):

```csharp
[Fact]
public async Task BookingsDbContext_Can_Persist_Booking()
{
    await using var context = new BookingsDbContext(_options);
    await context.Database.MigrateAsync();
    
    var booking = Booking.Create(Ulid.NewUlid(), "Test Customer");
    context.Bookings.Add(booking);
    await context.SaveChangesAsync();
    
    var stored = await context.Bookings.FindAsync(booking.Id);
    Assert.NotNull(stored);
}
```

## Deployment

See [Deployment Documentation](../Deployment/README.md) for Kubernetes and Docker deployment guides.

## See Also

- [Transponder Documentation](../Transponder/README.md) - Messaging framework
- [Intercessor Documentation](../Intercessor/README.md) - Mediator framework
- [Verifier Documentation](../Verifier/README.md) - Validation framework
