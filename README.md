# Sol9

Multi-project .NET solution with reusable libraries (Transponder, Intercessor, Verifier) and two ASP.NET Core web apps.

## Overview
- Two runnable web apps: `WebApplication1` and `WebApplication2`.
- Library projects for messaging/transports, persistence, and cross-cutting helpers.
- See `Intercessor/README.md` and `Verifier/README.md` for those libraries.

## Stack
- Language: C# (.NET)
- Frameworks: ASP.NET Core, gRPC, Entity Framework Core
- Package manager: NuGet via `dotnet restore`
- Tooling: Docker/Docker Compose (optional)

## Entry points
- `WebApplication1/Program.cs`
- `WebApplication2/Program.cs`

## Requirements
- .NET SDK 10.0.0 (see `global.json`)
- Optional: Docker Desktop (for `docker-compose.yml`)
- Optional: Postgres or SQL Server if you enable Transponder persistence

## Setup
```bash
dotnet restore
dotnet build Sol9.slnx
```

## Run
```bash
dotnet run --project WebApplication1/WebApplication1.csproj
dotnet run --project WebApplication2/WebApplication2.csproj
```

Default launch profiles map to:
- WebApplication1: `http://localhost:5026` (https: `https://localhost:7154`)
- WebApplication2: `http://localhost:5266` (https: `https://localhost:7111`)

## Run with Docker
```bash
docker compose up --build
```

Docker Compose publishes:
- WebApplication1: `5026` (HTTP), `5027` (gRPC)
- WebApplication2: `5266` (HTTP), `5267` (gRPC)

## Scripts (common dotnet CLI commands)
- Build: `dotnet build Sol9.slnx`
- Test: `dotnet test Sol9.slnx`
- Run app: `dotnet run --project WebApplication1/WebApplication1.csproj`

## Environment variables
The apps use standard ASP.NET Core configuration binding (`:` in JSON, `__` in env vars).

Common:
- `ASPNETCORE_ENVIRONMENT` (e.g. `Development`)
- `ASPNETCORE_URLS` (used to derive scheme/port for Transponder settings)

Transponder settings (from `TransponderSettings` and `appsettings.json`):
- `TransponderSettings__LocalBaseAddress`
- `TransponderSettings__RemoteBaseAddress`
- `TransponderSettings__RemoteAddressStrategy`
- `TransponderSettings__RemoteBaseAddresses__0__Url`
- `TransponderSettings__RemoteBaseAddresses__0__RemoteAddressStrategy`
- `TransponderSettings__LocalServiceName`
- `TransponderSettings__RemoteServiceName`
- `TransponderSettings__GrpcPortOffset`

Persistence connection strings (used by `WebApplication1`):
- `ConnectionStrings__TransponderPostgres`
- `ConnectionStrings__TransponderSqlServer`

See `WebApplication1/appsettings.json` and `WebApplication2/appsettings.json` for defaults.

## Tests
```bash
dotnet test Sol9.slnx
```

## Project structure
- `WebApplication1/`, `WebApplication1.Application/`, `WebApplication1.Domain/`, `WebApplication1.Infrastructure/`
- `WebApplication2/`, `WebApplication2.Application/`, `WebApplication2.Domain/`, `WebApplication2.Infrastructure/`
- `Transponder/` and `Transponder.*` libraries (contracts, transports, persistence, samples, logging)
- `Intercessor/` (mediator pipeline library)
- `Verifier/` (validation library)
- `Sol9.Eventing/` (eventing abstractions)
- `docker-compose.yml`, `global.json`, `Directory.Build.props`

## License
MIT License. See `LICENSE`.
