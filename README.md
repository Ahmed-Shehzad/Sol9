# Sol9

Multi-project .NET solution with a modular monolith layout: Bookings and Orders modules, shared libraries (Transponder, Intercessor, Verifier), and an Aspire-hosted API gateway.

## Overview
- Two runnable web apps: `Bookings.API` and `Orders.API`.
- Aspire AppHost orchestrates Bookings, Orders, PostgreSQL, Redis, and a YARP API gateway.
- Bookings/Orders communicate via Transponder gRPC with Serilog + OpenTelemetry scopes and PostgreSQL outbox/inbox.
- Library projects for messaging/transports, persistence, and cross-cutting helpers.
- See `Intercessor/README.md` and `Verifier/README.md` for those libraries.

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
