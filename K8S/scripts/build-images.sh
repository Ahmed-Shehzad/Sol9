#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SERVER_DIR="${ROOT_DIR}/.."
IMAGE_TAG="${IMAGE_TAG:-dev}"
REGISTRY_PREFIX="${REGISTRY_PREFIX:-}"
DOTNET_VERSION="${DOTNET_VERSION:-10.0}"

if [[ ! -d "${SERVER_DIR}" ]]; then
  echo "Server directory not found: ${SERVER_DIR}" >&2
  exit 1
fi

docker build \
  -f "${ROOT_DIR}/docker/Bookings.API.Dockerfile" \
  -t "${REGISTRY_PREFIX}sol9/bookings-api:${IMAGE_TAG}" \
  --build-arg DOTNET_VERSION="${DOTNET_VERSION}" \
  "${SERVER_DIR}"

docker build \
  -f "${ROOT_DIR}/docker/Orders.API.Dockerfile" \
  -t "${REGISTRY_PREFIX}sol9/orders-api:${IMAGE_TAG}" \
  --build-arg DOTNET_VERSION="${DOTNET_VERSION}" \
  "${SERVER_DIR}"

docker build \
  -f "${ROOT_DIR}/docker/Gateway.API.Dockerfile" \
  -t "${REGISTRY_PREFIX}sol9/gateway-api:${IMAGE_TAG}" \
  --build-arg DOTNET_VERSION="${DOTNET_VERSION}" \
  "${SERVER_DIR}"
