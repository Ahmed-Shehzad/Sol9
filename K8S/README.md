# Sol9 Kubernetes Setup (Full)

This is a clean, full setup: Kind + Docker + Helm + ArgoCD.
Use this to bring Sol9 up locally with a production-style workflow.

## What you get
- Kind cluster with local registry wiring
- Dockerfiles for Bookings, Orders, Gateway
- Helm chart for API services + Postgres + Redis
- ArgoCD project and application manifests

## Step-by-step (full setup)
1) Create the Kind cluster
```bash
./scripts/kind-up.sh
```

2) Build images
```bash
./scripts/build-images.sh
```

3) Load images into Kind
```bash
./scripts/kind-load.sh
```

4) Install ArgoCD
```bash
./scripts/argocd-install.sh
```

5) Apply ArgoCD manifests
```bash
kubectl apply -f argocd/sol9-project.yaml
kubectl apply -f argocd/sol9-application.yaml
```

6) Wait for workloads
```bash
kubectl -n sol9 get pods
```

7) Port-forward the Gateway and test
```bash
kubectl -n sol9 port-forward svc/sol9-gateway 8080:80
curl -i http://localhost:8080/bookings
curl -i http://localhost:8080/orders
```

## ArgoCD login (optional)
```bash
kubectl -n argocd port-forward svc/argocd-server 8080:80
ARGO_PWD=$(kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath='{.data.password}' | base64 -d)
argocd login localhost:8080 --username admin --password "$ARGO_PWD" --insecure
argocd app sync sol9
```

## Configuration notes
- Default images: `sol9/*:dev`
- Postgres and Redis run in-cluster. Update `helm/sol9/values.yaml` to use external services.
- For .NET base images, set `DOTNET_VERSION=8.0` if 10.0 is unavailable.

## Files
- `kind/cluster.yaml` Kind cluster config
- `docker/` Dockerfiles
- `helm/sol9/` Helm chart
- `argocd/` ArgoCD app + project
- `scripts/` helpers
