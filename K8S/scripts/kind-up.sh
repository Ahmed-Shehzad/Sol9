#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLUSTER_NAME="${CLUSTER_NAME:-sol9}"
REG_NAME="${REG_NAME:-kind-registry}"
REG_PORT="${REG_PORT:-5001}"

if ! docker ps -a --format '{{.Names}}' | grep -q "^${REG_NAME}$"; then
  docker run -d --restart=always -p "${REG_PORT}:5000" --name "${REG_NAME}" registry:2
fi

kind create cluster --name "${CLUSTER_NAME}" --config "${ROOT_DIR}/kind/cluster.yaml"

docker network connect kind "${REG_NAME}" >/dev/null 2>&1 || true

kubectl apply -f - <<EOF_CM
apiVersion: v1
kind: ConfigMap
metadata:
  name: local-registry-hosting
  namespace: kube-public
data:
  localRegistryHosting.v1: |
    host: "localhost:${REG_PORT}"
    help: "https://kind.sigs.k8s.io/docs/user/local-registry/"
EOF_CM
