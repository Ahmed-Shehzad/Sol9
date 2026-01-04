# Sol9

Multi-project .NET solution with a modular monolith layout: Bookings and Orders modules, shared libraries (Transponder, Intercessor, Verifier), and an Aspire-hosted API gateway.

## Overview
- Two runnable web apps: `Bookings.API` and `Orders.API`.
- Aspire AppHost orchestrates Bookings, Orders, PostgreSQL, Redis, and a YARP API gateway.
- Bookings/Orders communicate via Transponder gRPC with Serilog + OpenTelemetry scopes and PostgreSQL outbox/inbox.
- Library projects for messaging/transports, persistence, and cross-cutting helpers.
- See `Intercessor/README.md` and `Verifier/README.md` for those libraries.

## Libraries usage guide
This solution ships with several internal libraries. Each question below includes a short code snippet.

### Transponder (messaging, gRPC, outbox)
#### What to use?
Use `IRequestClient<T>`, `IPublishEndpoint`, and `ISagaMessageHandler<TState, TMessage>`.
```csharp
IRequestClient<CreateBookingRequest> client =
    _clientFactory.CreateRequestClient<CreateBookingRequest>();
```

#### How to use?
Register Transponder + gRPC in `Program.cs`, then map the gRPC service.
```csharp
builder.Services.AddTransponder(localAddress, options =>
{
    options.TransportBuilder.UseGrpc(localAddress, remoteAddresses);
    options.UseOutbox();
});
app.MapGrpcService<GrpcTransportService>();
```

#### When to use?
Use it for cross-module workflows or request/response between services.
```csharp
CreateBookingResponse response = await client
    .GetResponseAsync<CreateBookingResponse>(new CreateBookingRequest(order.Id, order.CustomerName), ct);
```

#### Where to use?
Transport config lives in API startup; requests live in Application handlers.
```csharp
// API startup
ConfigureTransponder(builder);

// Application handler
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto> { }
```

#### Why to use?
It provides resilience and transport abstraction (outbox, retries, gRPC).
```csharp
options.UseOutbox();
options.UsePersistedMessageScheduler();
```

### Intercessor (mediator + pipeline)
#### What to use?
Use commands, queries, and handlers.
```csharp
public sealed record GetOrdersQuery() : IQuery<IReadOnlyList<OrderDto>>;
public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IReadOnlyList<OrderDto>> { }
```

#### How to use?
Register Intercessor and scan the Application assembly.
```csharp
services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(OrdersApplication).Assembly);
});
```

#### When to use?
Use it when a controller should delegate to application logic.
```csharp
public async Task<IActionResult> GetAsync() =>
    Ok(await _sender.SendAsync(new GetOrdersQuery()));
```

#### Where to use?
Handlers and behaviors live in Application, controllers in API.
```csharp
// Orders.Application
public sealed class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, OrderDto> { }
```

#### Why to use?
It centralizes cross-cutting concerns via pipeline behaviors.
```csharp
services.AddIntercessor(options =>
{
    options.RegisterFromAssembly(typeof(OrdersApplication).Assembly);
    options.AddBehavior<LoggingBehavior<,>>();
});
```

### Verifier (validation)
#### What to use?
Use `AbstractValidator<T>` and rule definitions.
```csharp
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand> { }
```

#### How to use?
Define rules per request.
```csharp
_ = RuleFor(x => x.TotalAmount)
    .Must(amount => amount > 0, "TotalAmount must be greater than zero.");
```

#### When to use?
Use validators for all commands/queries that accept input.
```csharp
public sealed record CreateOrderCommand(string CustomerName, decimal TotalAmount) : ICommand<OrderDto>;
```

#### Where to use?
Validators live in Application next to the command/query.
```csharp
// Orders.Application/Commands/CreateOrder/CreateOrderCommand.cs
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand> { }
```

#### Why to use?
Consistent validation ensures uniform error handling.
```csharp
// Intercessor registers ValidationBehavior automatically.
```

### Sol9.Contracts (shared message contracts)
#### What to use?
Use shared message records for cross-module calls.
```csharp
public sealed record CreateBookingRequest(Ulid OrderId, string CustomerName) : ICorrelatedMessage;
```

#### How to use?
Reference the contracts package/project from both producer and consumer.
```csharp
using Sol9.Contracts.Bookings;
```

#### When to use?
When a message crosses module boundaries.
```csharp
await client.GetResponseAsync<CreateBookingResponse>(new CreateBookingRequest(order.Id, order.CustomerName), ct);
```

#### Where to use?
Define in `Sol9.Contracts`, reference from APIs/Application.
```csharp
// Sol9.Contracts/Bookings/CreateBookingRequest.cs
```

#### Why to use?
Shared contracts prevent schema drift between services.
```csharp
// Single source of truth for message shape.
```

### Sol9.ServiceDefaults (host defaults)
#### What to use?
Use `AddServiceDefaults()` for logging/telemetry defaults.
```csharp
builder.AddServiceDefaults();
```

#### How to use?
Call it once in each API `Program.cs`.
```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
```

#### When to use?
Always for hosted APIs (Orders/Bookings/Gateway).
```csharp
// Program.cs
builder.AddServiceDefaults();
```

#### Where to use?
At API startup before registering custom services.
```csharp
builder.AddServiceDefaults();
builder.Services.AddControllers();
```

#### Why to use?
It standardizes diagnostics and configuration conventions.
```csharp
// Consistent telemetry/logging across all services.
```

## Best practices and recommendations
- Prefer HTTPS endpoints for gRPC so HTTP/2 is enabled and reliable.
```csharp
_ = bookingsApi.WithEnvironment("TransponderDefaults__LocalAddress", bookingsApi.GetEndpoint("https"));
```
- Keep handlers small: orchestration in Application, persistence in Infrastructure, HTTP in API.
- Use outbox for cross-module messaging and make handlers idempotent.
```csharp
options.UseOutbox();
```
- Ensure only one validator registration per request to avoid duplicate errors.
- Put message types in `Sol9.Contracts` and keep them backward compatible.
- Use OpenTelemetry scopes and `Activity.AddException` for failure visibility.

## Stack
- Language: C# (.NET)
- Frameworks: ASP.NET Core, gRPC, Entity Framework Core, .NET Aspire, YARP, Transponder
- Package manager: NuGet via `dotnet restore`
- Tooling: dotnet CLI, Testcontainers (PostgreSQL), Redis

## Entry points
- `Bookings.API/Program.cs`
- `Orders.API/Program.cs`
- `Gateway.API/Program.cs`
- `Sol9.AppHost/Program.cs`

## Requirements
- .NET SDK 10.0.0 (see `global.json`)
- Docker (for Aspire Postgres + Redis containers and Testcontainers integration tests)
- PostgreSQL for Orders/Bookings + Transponder persistence

## Setup
```bash
dotnet restore
dotnet build Sol9.slnx
```

## Run (Aspire)
```bash
dotnet run --project Sol9.AppHost/Sol9.AppHost.csproj
```

Aspire runs Bookings, Orders, Gateway, PostgreSQL, and Redis. Use the dashboard to see assigned ports and service health.

Redis TLS (local dev):
- Certs are generated under `Sol9.AppHost/certs/redis` and mounted into the Redis container.
- Clients use `rediss://` connection strings; Development allows self-signed certs for local runs.

Gateway routes:
- `/bookings/*` -> Bookings.API
- `/orders/*` -> Orders.API

Transponder flow:
- Orders.API sends `CreateBookingRequest` to Bookings.API over gRPC and awaits `CreateBookingResponse`.

## Run (individual services)
```bash
dotnet run --project Bookings.API/Bookings.API.csproj
dotnet run --project Orders.API/Orders.API.csproj
```

Default launch profiles map to:
- Bookings.API: `http://localhost:5187` (https: `https://localhost:7266`)
- Orders.API: `http://localhost:5296` (https: `https://localhost:7268`)
- Gateway.API: `http://localhost:5400` (https: `https://localhost:7440`)

## Scripts (common dotnet CLI commands)
- Build: `dotnet build Sol9.slnx`
- Test: `dotnet test Sol9.slnx`
- Run app: `dotnet run --project Bookings.API/Bookings.API.csproj`
- Run Aspire host: `dotnet run --project Sol9.AppHost/Sol9.AppHost.csproj`

## Environment variables
The apps use standard ASP.NET Core configuration binding (`:` in JSON, `__` in env vars).

Common:
- `ASPNETCORE_ENVIRONMENT` (e.g. `Development`)
- `ASPNETCORE_URLS` (used to derive scheme/port for Transponder settings)

Transponder settings (gRPC transport on the same port):
- `TransponderSettings__LocalBaseAddress`
- `TransponderSettings__RemoteBaseAddress`
- `TransponderSettings__RemoteAddressStrategy`
- `TransponderSettings__RemoteBaseAddresses__0__Url`
- `TransponderSettings__RemoteBaseAddresses__0__RemoteAddressStrategy`
- `TransponderSettings__LocalServiceName`
- `TransponderSettings__RemoteServiceName`
- `TransponderSettings__GrpcPortOffset`

Transponder persistence (PostgreSQL):
- `ConnectionStrings__Transponder`
- `TransponderPersistence__Schema`

Orders/Bookings connection strings:
- `ConnectionStrings__Orders`
- `ConnectionStrings__Bookings`
- `ConnectionStrings__Transponder`
- `ConnectionStrings__Redis`

See `Bookings.API/appsettings.json`, `Orders.API/appsettings.json`, and `Gateway.API/appsettings.json` for base config and defaults.

## Tests
```bash
dotnet test Sol9.slnx
```

Testcontainers integration tests require Docker:
```bash
dotnet test Bookings.IntegrationTests/Bookings.IntegrationTests.csproj
dotnet test Orders.IntegrationTests/Orders.IntegrationTests.csproj
```

## Project structure
- `Bookings.API/`, `Bookings.Application/`, `Bookings.Domain/`, `Bookings.Infrastructure/`
- `Orders.API/`, `Orders.Application/`, `Orders.Domain/`, `Orders.Infrastructure/`
- `Gateway.API/` (YARP reverse proxy and API gateway)
- `Sol9.Contracts/` (shared Transponder message contracts)
- `Sol9.Core/` (domain and integration event abstractions)
- `Sol9.AppHost/` and `Sol9.ServiceDefaults/` (.NET Aspire host and defaults)
- `Transponder/` and `Transponder.*` libraries (contracts, transports, persistence, samples, logging)
- `Intercessor/` (mediator pipeline library)
- `Verifier/` (validation library)
- `Bookings.IntegrationTests/` (PostgreSQL Testcontainers)
- `Orders.IntegrationTests/` (PostgreSQL Testcontainers)
- `global.json`, `Directory.Build.props`, `Sol9.slnx`

## License
MIT License. See `LICENSE`.
