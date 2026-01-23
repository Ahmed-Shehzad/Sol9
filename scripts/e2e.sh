#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
COMPOSE_FILE="$ROOT_DIR/docker-compose.e2e.yml"
LOG_DIR="$ROOT_DIR/artifacts/e2e-logs"

PIDS=()

cleanup() {
  for pid in "${PIDS[@]:-}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" 2>/dev/null || true
    fi
  done

  docker compose -f "$COMPOSE_FILE" down -v >/dev/null 2>&1 || true
}
trap cleanup EXIT

dump_logs() {
  if [ ! -d "$LOG_DIR" ]; then
    return 0
  fi

  for log_file in "$LOG_DIR"/*.log; do
    if [ -f "$log_file" ]; then
      echo "----- $log_file -----" >&2
      tail -n 200 "$log_file" >&2 || true
    fi
  done
}

wait_for_url() {
  local url=$1
  local attempts=${2:-60}
  local delay=${3:-2}

  for _ in $(seq 1 "$attempts"); do
    if curl -fsS "$url" >/dev/null 2>&1; then
      return 0
    fi
    sleep "$delay"
  done

  echo "Timed out waiting for $url" >&2
  dump_logs
  return 1
}

wait_for_health() {
  local service=$1
  local attempts=${2:-30}
  local delay=${3:-2}

  for _ in $(seq 1 "$attempts"); do
    local cid
    cid=$(docker compose -f "$COMPOSE_FILE" ps -q "$service" 2>/dev/null || true)
    if [ -n "$cid" ]; then
      local status
      status=$(docker inspect -f '{{.State.Health.Status}}' "$cid" 2>/dev/null || true)
      if [ "$status" = "healthy" ]; then
        return 0
      fi
    fi
    sleep "$delay"
  done

  echo "Timed out waiting for $service to become ready" >&2
  docker compose -f "$COMPOSE_FILE" ps >&2 || true
  dump_logs
  return 1
}

if ! command -v docker >/dev/null 2>&1; then
  echo "Docker is required to run E2E tests." >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo ".NET SDK is required to run E2E tests." >&2
  exit 1
fi

if [ "${E2E_SKIP_BUILD:-}" != "1" ]; then
  dotnet build "$ROOT_DIR/Sol9.slnx" -c Release
fi

docker compose -f "$COMPOSE_FILE" up -d

wait_for_health postgres
wait_for_health redis

export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_ENVIRONMENT=Development
mkdir -p "$LOG_DIR"

# Start centralized Transponder gRPC service on port 50051
ASPNETCORE_URLS=https://localhost:50051 \
Kestrel__Endpoints__Grpc__Url=https://localhost:50051 \
Kestrel__Endpoints__Grpc__Protocols=Http2 \
  dotnet run --project "$ROOT_DIR/Transponder.Service" -c Release --no-build --no-launch-profile >"$LOG_DIR/transponder-service.log" 2>&1 &
PIDS+=("$!")

# Wait for Transponder service to be ready
wait_for_url "https://localhost:50051/health" 60 2

# Start Orders.API - connects to centralized Transponder service
ASPNETCORE_URLS=http://localhost:5296 \
ConnectionStrings__Orders="Host=localhost;Database=orders;Username=postgres;Password=postgres" \
ConnectionStrings__Transponder="Host=localhost;Database=orders;Username=postgres;Password=postgres" \
ConnectionStrings__Redis="redis://localhost:6379" \
TransponderDefaults__LocalAddress=http://localhost:5296 \
TransponderDefaults__RemoteAddress=https://localhost:50051 \
  dotnet run --project "$ROOT_DIR/Orders.API" -c Release --no-build --no-launch-profile >"$LOG_DIR/orders-api.log" 2>&1 &
PIDS+=("$!")

# Start Bookings.API - connects to centralized Transponder service
ASPNETCORE_URLS=http://localhost:5187 \
ConnectionStrings__Bookings="Host=localhost;Database=bookings;Username=postgres;Password=postgres" \
ConnectionStrings__Transponder="Host=localhost;Database=bookings;Username=postgres;Password=postgres" \
ConnectionStrings__Redis="redis://localhost:6379" \
TransponderDefaults__LocalAddress=http://localhost:5187 \
TransponderDefaults__RemoteAddress=https://localhost:50051 \
  dotnet run --project "$ROOT_DIR/Bookings.API" -c Release --no-build --no-launch-profile >"$LOG_DIR/bookings-api.log" 2>&1 &
PIDS+=("$!")

# Start Gateway.API
ASPNETCORE_URLS=http://localhost:18080 \
  dotnet run --project "$ROOT_DIR/Gateway.API" -c Release --no-build --no-launch-profile >"$LOG_DIR/gateway-api.log" 2>&1 &
PIDS+=("$!")

wait_for_url "http://localhost:18080/alive" 60 2

export E2E_BASE_URL=http://localhost:18080
export TRANSPONDER_SERVICE_URL=https://localhost:50051

# Run E2E tests
dotnet test "$ROOT_DIR/Orders.E2E.Tests" -c Release --no-build --verbosity normal
dotnet test "$ROOT_DIR/Bookings.E2E.Tests" -c Release --no-build --verbosity normal
dotnet test "$ROOT_DIR/Transponder.Service.Tests" -c Release --no-build --verbosity normal
