#!/usr/bin/env bash
set -euo pipefail

CLUSTER_NAME="${CLUSTER_NAME:-sol9}"
REG_NAME="${REG_NAME:-kind-registry}"

kind delete cluster --name "${CLUSTER_NAME}"

docker rm -f "${REG_NAME}" >/dev/null 2>&1 || true
