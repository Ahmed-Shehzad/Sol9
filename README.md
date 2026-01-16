# Sol9

A modern .NET modular monolith solution demonstrating enterprise messaging patterns, domain-driven design, and microservices communication using the Transponder messaging framework.

## Overview

Sol9 is a comprehensive solution that showcases:

- **Modular Monolith Architecture**: Bookings and Orders modules with clear boundaries
- **Enterprise Messaging**: Transponder framework for reliable inter-module communication
- **Multiple Transport Options**: gRPC, SignalR, SSE, Webhooks, Kafka, RabbitMQ, AWS, Azure Service Bus
- **Resilience Patterns**: Outbox/inbox patterns, saga orchestration, message scheduling
- **Modern .NET Stack**: .NET 10, ASP.NET Core, Entity Framework Core, .NET Aspire
- **Observability**: OpenTelemetry, Serilog, structured logging

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Gateway.API (YARP)                        │
│              API Gateway & Reverse Proxy                    │
└──────────────┬──────────────────────────┬───────────────────┘
               │                          │
    ┌──────────▼──────────┐    ┌──────────▼──────────┐
    │   Bookings.API      │    │    Orders.API       │
    │  (gRPC + HTTP)      │    │  (gRPC + HTTP)      │
    └──────────┬──────────┘    └──────────┬──────────┘
               │                          │
    ┌──────────▼──────────┐    ┌──────────▼──────────┐
    │ Bookings.Application│    │ Orders.Application  │
    │  (Domain Logic)      │    │  (Domain Logic)     │
    └──────────┬──────────┘    └──────────┬──────────┘
               │                          │
    ┌──────────▼──────────┐    ┌──────────▼──────────┐
    │ Bookings.Infrastructure│ │ Orders.Infrastructure│
    │  (EF Core + Repos)   │  │  (EF Core + Repos)   │
    └──────────────────────┘  └──────────────────────┘
               │                          │
               └──────────┬───────────────┘
                          │
              ┌───────────▼───────────┐
              │   Transponder         │
              │  (Messaging Bus)      │
              └───────────┬───────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
   ┌────▼────┐      ┌─────▼─────┐    ┌─────▼─────┐
   │  gRPC   │      │  SignalR  │    │   Kafka  │
   │Transport│      │ Transport │    │Transport │
   └─────────┘      └───────────┘    └──────────┘
```

## Key Components

### Core Libraries

- **Transponder**: Enterprise messaging framework with multiple transport support
- **Intercessor**: Mediator pattern implementation with pipeline behaviors
- **Verifier**: Validation framework with fluent rule builders
- **Sol9.Core**: Shared domain abstractions and integration events
- **Sol9.Contracts**: Shared message contracts for inter-module communication

### Application Modules

- **Bookings.API**: Booking management service
- **Orders.API**: Order management service
- **Gateway.API**: YARP-based API gateway

### Infrastructure

- **PostgreSQL**: Primary database for Orders, Bookings, and Transponder persistence
- **Redis**: Caching and distributed coordination
- **.NET Aspire**: Application orchestration and observability

## Quick Start

### Prerequisites

- .NET SDK 10.0.0 (see `global.json`)
- Docker Desktop (for Aspire containers and integration tests)
- PostgreSQL (optional, if not using Docker)

### Setup

```bash
# Clone and restore
git clone <repository-url>
cd Sol9
dotnet restore

# Build solution
dotnet build Sol9.slnx
```

### Run with Aspire (Recommended)

```bash
dotnet run --project Sol9.AppHost/Sol9.AppHost.csproj
```

This starts:
- Bookings.API
- Orders.API
- Gateway.API
- PostgreSQL
- Redis

Access the Aspire dashboard at `https://localhost:15000` to view service health, logs, and metrics.

### Run Individual Services

```bash
# Bookings API
dotnet run --project Bookings.API/Bookings.API.csproj

# Orders API
dotnet run --project Orders.API/Orders.API.csproj

# Gateway API
dotnet run --project Gateway.API/Gateway.API.csproj
```

Default ports:
- Bookings.API: `http://localhost:5187` / `https://localhost:7266`
- Orders.API: `http://localhost:5296` / `https://localhost:7268`
- Gateway.API: `http://localhost:5400` / `https://localhost:7440`

## Documentation

Comprehensive documentation is available in the `docs/` directory:

- **[Transponder Messaging Framework](docs/Transponder/README.md)**: Core messaging system, transports, and patterns
- **[Intercessor](docs/Intercessor/README.md)**: Mediator pattern and pipeline behaviors
- **[Verifier](docs/Verifier/README.md)**: Validation framework
- **[Applications](docs/Applications/README.md)**: Bookings and Orders module documentation
- **[Deployment](docs/Deployment/README.md)**: Kubernetes and Docker deployment guides

## Transports

Transponder supports multiple transport implementations:

- **gRPC**: High-performance RPC for service-to-service communication
- **SignalR**: Real-time bidirectional communication for web clients
- **SSE**: Server-Sent Events for one-way real-time updates
- **Webhooks**: HTTP-based webhook delivery
- **Kafka**: Distributed event streaming
- **RabbitMQ**: Message broker with advanced routing
- **AWS SQS/SNS**: Cloud messaging via AWS
- **Azure Service Bus**: Cloud messaging via Azure

See [Transponder Transports Documentation](docs/Transponder/Transports/README.md) for details.

## Features

### Messaging Patterns

- **Request/Response**: Synchronous communication between services
- **Publish/Subscribe**: Event-driven messaging
- **Saga Orchestration**: Distributed transaction coordination
- **Outbox Pattern**: Reliable message delivery with transactional guarantees
- **Inbox Pattern**: Idempotent message processing
- **Message Scheduling**: Delayed and scheduled message delivery

### Resilience

- **Retry Policies**: Configurable retry with exponential backoff
- **Circuit Breakers**: Automatic failure detection and recovery
- **Dead-Letter Queues**: Handling of unprocessable messages
- **Optimistic Concurrency**: Saga state versioning for conflict resolution

### Observability

- **Structured Logging**: Serilog with correlation IDs
- **Distributed Tracing**: OpenTelemetry integration
- **Metrics**: Performance and health metrics
- **Activity Scopes**: Automatic context propagation

## Testing

```bash
# Run all tests
dotnet test Sol9.slnx

# Run unit tests only
dotnet test Transponder.Tests/Transponder.Tests.csproj

# Run integration tests (requires Docker)
dotnet test Bookings.Integration.Tests/Bookings.Integration.Tests.csproj
dotnet test Orders.Integration.Tests/Orders.Integration.Tests.csproj

# Run E2E tests
dotnet test Bookings.E2E.Tests/Bookings.E2E.Tests.csproj
```

## Configuration

Configuration uses standard ASP.NET Core patterns:

- **appsettings.json**: Base configuration
- **appsettings.{Environment}.json**: Environment-specific overrides
- **Environment Variables**: Override any setting using `__` separator

### Key Settings

```json
{
  "ConnectionStrings": {
    "Bookings": "Host=localhost;Database=bookings;Username=postgres;Password=postgres",
    "Orders": "Host=localhost;Database=orders;Username=postgres;Password=postgres",
    "Transponder": "Host=localhost;Database=transponder;Username=postgres;Password=postgres",
    "Redis": "redis://localhost:6379"
  },
  "TransponderSettings": {
    "LocalBaseAddress": "http://localhost:5187",
    "RemoteBaseAddress": "http://localhost:5296"
  }
}
```

## Project Structure

```
Sol9/
├── Bookings.API/              # Bookings web API
├── Bookings.Application/       # Bookings domain logic
├── Bookings.Domain/            # Bookings domain entities
├── Bookings.Infrastructure/    # Bookings data access
├── Orders.API/                 # Orders web API
├── Orders.Application/         # Orders domain logic
├── Orders.Domain/             # Orders domain entities
├── Orders.Infrastructure/      # Orders data access
├── Gateway.API/                # API Gateway (YARP)
├── Transponder/                # Core messaging framework
├── Transponder.Transports.*/   # Transport implementations
├── Transponder.Persistence.*/  # Persistence implementations
├── Intercessor/                # Mediator library
├── Verifier/                   # Validation library
├── Sol9.Contracts/             # Shared message contracts
├── Sol9.Core/                  # Shared domain abstractions
├── Sol9.AppHost/               # Aspire orchestration
└── Sol9.ServiceDefaults/       # Service defaults
```

## Best Practices

1. **Use Outbox Pattern**: Always use outbox for cross-module messaging to ensure reliability
2. **Idempotent Handlers**: Make message handlers idempotent to handle duplicates
3. **Shared Contracts**: Keep message contracts in `Sol9.Contracts` for versioning
4. **Structured Logging**: Use correlation IDs for request tracing
5. **Validation**: Validate all commands/queries using Verifier
6. **HTTPS for gRPC**: Use HTTPS endpoints to enable HTTP/2

## License

MIT License. See `LICENSE` file for details.

## Contributing

Contributions are welcome! Please ensure:

- Code follows existing patterns and conventions
- Tests are included for new features
- Documentation is updated
- All tests pass before submitting

## Support

For questions and issues, please open an issue in the repository.
