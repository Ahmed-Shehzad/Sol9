#!/usr/bin/env bash
set -euo pipefail

CLUSTER_NAME="${CLUSTER_NAME:-sol9}"
IMAGE_TAG="${IMAGE_TAG:-dev}"
REGISTRY_PREFIX="${REGISTRY_PREFIX:-}"

images=(
  "${REGISTRY_PREFIX}sol9/bookings-api:${IMAGE_TAG}"
  "${REGISTRY_PREFIX}sol9/orders-api:${IMAGE_TAG}"
  "${REGISTRY_PREFIX}sol9/gateway-api:${IMAGE_TAG}"
)

for image in "${images[@]}"; do
  kind load docker-image --name "${CLUSTER_NAME}" "${image}"
done
