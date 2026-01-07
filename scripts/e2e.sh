#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
COMPOSE_FILE="$ROOT_DIR/docker-compose.e2e.yml"

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

export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_ENVIRONMENT=Development

ASPNETCORE_URLS=http://localhost:5296 \
ConnectionStrings__Orders="Host=localhost;Database=orders;Username=postgres;Password=postgres" \
ConnectionStrings__Transponder="Host=localhost;Database=orders;Username=postgres;Password=postgres" \
ConnectionStrings__Redis="redis://localhost:6379" \
TransponderDefaults__LocalAddress=http://localhost:5296 \
TransponderDefaults__RemoteAddress=http://localhost:5187 \
  dotnet run --project "$ROOT_DIR/Orders.API" -c Release --no-build >/dev/null 2>&1 &
PIDS+=("$!")

ASPNETCORE_URLS=http://localhost:5187 \
ConnectionStrings__Bookings="Host=localhost;Database=bookings;Username=postgres;Password=postgres" \
ConnectionStrings__Transponder="Host=localhost;Database=bookings;Username=postgres;Password=postgres" \
ConnectionStrings__Redis="redis://localhost:6379" \
TransponderDefaults__LocalAddress=http://localhost:5187 \
TransponderDefaults__RemoteAddress=http://localhost:5296 \
  dotnet run --project "$ROOT_DIR/Bookings.API" -c Release --no-build >/dev/null 2>&1 &
PIDS+=("$!")

ASPNETCORE_URLS=http://localhost:18080 \
  dotnet run --project "$ROOT_DIR/Gateway.API" -c Release --no-build >/dev/null 2>&1 &
PIDS+=("$!")

wait_for_url "http://localhost:18080/alive" 60 2

export E2E_BASE_URL=http://localhost:18080

dotnet test "$ROOT_DIR/Orders.E2E.Tests" -c Release --no-build --verbosity normal

dotnet test "$ROOT_DIR/Bookings.E2E.Tests" -c Release --no-build --verbosity normal
