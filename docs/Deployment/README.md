# Deployment Guide

This guide covers deploying Sol9 applications to various environments using Docker and Kubernetes, including blue-green deployment with Argo Rollouts.

## Prerequisites

- Docker Desktop (for local development)
- Kubernetes cluster (for production)
- kubectl configured
- Docker registry access (for container images)
- **Argo Rollouts** controller (for blue-green deployments; see [Blue-Green Deployment](#blue-green-deployment))

## Docker Deployment

### Building Images

Dockerfiles are provided for each API. Build from the **repository root**:

```bash
# Build all images
docker build -t sol9/bookings-api:latest -f Bookings.API/Dockerfile .
docker build -t sol9/orders-api:latest -f Orders.API/Dockerfile .
docker build -t sol9/gateway-api:latest -f Gateway.API/Dockerfile .

# Or with a version tag (e.g. commit SHA)
docker build -t <registry>/sol9/bookings-api:<tag> -f Bookings.API/Dockerfile .
docker build -t <registry>/sol9/orders-api:<tag> -f Orders.API/Dockerfile .
docker build -t <registry>/sol9/gateway-api:<tag> -f Gateway.API/Dockerfile .
```

### Running with Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Docker Compose Configuration

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7
    ports:
      - "6379:6379"

  bookings-api:
    image: sol9/bookings-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Bookings=Host=postgres;Database=bookings;...
      - ConnectionStrings__Transponder=Host=postgres;Database=transponder;...
    depends_on:
      - postgres
      - redis
    ports:
      - "5187:8080"

  orders-api:
    image: sol9/orders-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Orders=Host=postgres;Database=orders;...
    depends_on:
      - postgres
    ports:
      - "5296:8080"

  gateway-api:
    image: sol9/gateway-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - bookings-api
      - orders-api
    ports:
      - "5400:8080"

volumes:
  postgres_data:
```

## Kubernetes Deployment

### Namespace

```bash
kubectl create namespace sol9
```

### gRPC and Transponder Service Configuration

Bookings.API and Orders.API communicate via **gRPC** (Transponder transport). In Kubernetes, set the following so each service resolves the other via **K8s Service DNS**:

- **TransponderDefaults__LocalAddress** – this service’s base URL (e.g. `http://<bookings-api-service>:80`).
- **TransponderDefaults__RemoteAddress** – the other service’s base URL (e.g. `http://<orders-api-service>:80`).

When using Helm, service names follow the release and chart (e.g. release `sol9-bookings`, chart `bookings-api` → service `sol9-bookings-bookings-api`). Configure in your values or ConfigMap:

```yaml
# Example: for Bookings.API (replace namespace/release as needed)
env:
  transponderDefaults:
    localAddress: "http://sol9-bookings-bookings-api:80"
    remoteAddress: "http://sol9-orders-orders-api:80"
```

For Orders.API, swap local and remote. If you use a **dedicated gRPC port**, set `service.grpcPort` and `service.grpcTargetPort` in the Helm values and use the same service name with the gRPC port in the URL.

### ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: sol9-config
  namespace: sol9
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  TransponderSettings__LocalBaseAddress: "http://bookings-api:80"
  TransponderSettings__RemoteBaseAddress: "http://orders-api:80"
```

### Secrets

```bash
# Create secrets
kubectl create secret generic sol9-secrets \
  --from-literal=postgres-password=your-password \
  --from-literal=redis-password=your-password \
  -n sol9
```

### Deploying with Helm

Deploy **backends first**, then the gateway:

```bash
# Backend APIs (order can be parallel)
helm upgrade --install sol9-bookings ./K8s/helm/bookings-api -n sol9 -f values.prod.yaml --set image.tag=<tag>
helm upgrade --install sol9-orders   ./K8s/helm/orders-api   -n sol9 -f values.prod.yaml --set image.tag=<tag>

# Gateway (after backends)
helm upgrade --install sol9-gateway ./K8s/helm/gateway-api  -n sol9 -f values.prod.yaml --set image.tag=<tag>
```

## Blue-Green Deployment

Blue-green is implemented with **Argo Rollouts** in the Helm charts. The active Service keeps the same name; promotion switches traffic from blue (current) to green (new) ReplicaSet so **HTTP and gRPC** both move to the new version without changing DNS.

### Prerequisites

- **Argo Rollouts** controller installed in the cluster:
  ```bash
  kubectl create namespace argo-rollouts
  kubectl apply -n argo-rollouts -f https://github.com/argoproj/rollouts-operator/releases/latest/download/install.yaml
  ```
- Optional: **kubectl argo rollouts** plugin for CLI:
  ```bash
  curl -LO https://github.com/argoproj/rollouts-operator/releases/latest/download/kubectl-argo-rollouts-linux-amd64
  chmod +x kubectl-argo-rollouts-linux-amd64 && sudo mv kubectl-argo-rollouts-linux-amd64 /usr/local/bin/kubectl-argo-rollouts
  ```

### Enabling Blue-Green in Helm

In your values (e.g. `values.prod.yaml`) for **bookings-api**, **orders-api**, and **gateway-api**:

```yaml
rollout:
  enabled: true
  strategy: blueGreen
  blueGreen:
    autoPromotionEnabled: false   # manual promote
    previewReplicaCount: 1
    scaleDownDelaySeconds: 30
```

Optional: enable analysis (e.g. Prometheus success rate) via `rollout.analysis` in values.

### Blue-Green Flow

1. **Deploy new version (green)**  
   `helm upgrade ... --set image.tag=<new-tag>`. Argo creates a new ReplicaSet (green) and keeps the active Service pointing at blue.

2. **Validate green**  
   Use the **preview** Service (e.g. `sol9-bookings-bookings-api-preview`) for HTTP (and gRPC if `service.grpcPort` is set). Run smoke tests or wait for automated analysis.

3. **Promote**  
   When satisfied:
   ```bash
   kubectl argo rollouts promote <rollout-name> -n sol9
   ```
   Rollout names follow Helm fullname: `sol9-bookings-bookings-api`, `sol9-orders-orders-api`, `sol9-gateway-gateway-api`.

4. **Rollback (if needed)**  
   Abort the rollout or revert:
   ```bash
   kubectl argo rollouts abort <rollout-name> -n sol9
   ```
   Or revert to a previous Helm revision and run `helm upgrade` again; Argo can roll back to the previous ReplicaSet.

### PostgreSQL Deployment

```yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgres
  namespace: sol9
spec:
  serviceName: postgres
  replicas: 1
  selector:
    matchLabels:
      app: postgres
  template:
    metadata:
      labels:
        app: postgres
    spec:
      containers:
      - name: postgres
        image: postgres:16
        env:
        - name: POSTGRES_USER
          value: postgres
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: sol9-secrets
              key: postgres-password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgres-data
          mountPath: /var/lib/postgresql/data
  volumeClaimTemplates:
  - metadata:
      name: postgres-data
    spec:
      accessModes: [ "ReadWriteOnce" ]
      resources:
        requests:
          storage: 10Gi
---
apiVersion: v1
kind: Service
metadata:
  name: postgres
  namespace: sol9
spec:
  selector:
    app: postgres
  ports:
  - port: 5432
    targetPort: 5432
```

### Bookings API Deployment

When `rollout.enabled` is false, the chart renders a standard Deployment. When true, it renders an Argo Rollout with blue-green strategy (see [Blue-Green Deployment](#blue-green-deployment)).

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bookings-api
  namespace: sol9
spec:
  replicas: 3
  selector:
    matchLabels:
      app: bookings-api
  template:
    metadata:
      labels:
        app: bookings-api
    spec:
      containers:
      - name: bookings-api
        image: sol9/bookings-api:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: sol9-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__Bookings
          value: "Host=postgres;Database=bookings;Username=postgres;Password=$(POSTGRES_PASSWORD)"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: sol9-secrets
              key: postgres-password
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: bookings-api
  namespace: sol9
spec:
  selector:
    app: bookings-api
  ports:
  - port: 80
    targetPort: 8080
  type: ClusterIP
```

### Orders API Deployment

Similar to Bookings API deployment (adjust names and ports).

### Gateway API Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gateway-api
  namespace: sol9
spec:
  replicas: 2
  selector:
    matchLabels:
      app: gateway-api
  template:
    metadata:
      labels:
        app: gateway-api
    spec:
      containers:
      - name: gateway-api
        image: sol9/gateway-api:latest
        ports:
        - containerPort: 8080
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "200m"
---
apiVersion: v1
kind: Service
metadata:
  name: gateway-api
  namespace: sol9
spec:
  selector:
    app: gateway-api
  ports:
  - port: 80
    targetPort: 8080
  type: LoadBalancer
```

### Ingress

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: sol9-ingress
  namespace: sol9
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - api.example.com
    secretName: sol9-tls
  rules:
  - host: api.example.com
    http:
      paths:
      - path: /bookings
        pathType: Prefix
        backend:
          service:
            name: bookings-api
            port:
              number: 80
      - path: /orders
        pathType: Prefix
        backend:
          service:
            name: orders-api
            port:
              number: 80
```

## Health Checks

All services expose health check endpoints:

```
GET /health   # Liveness probe
GET /ready    # Readiness probe
```

Helm charts use `/alive` by default for probes; adjust in values if needed.

## Monitoring

### Prometheus Metrics

Services expose metrics at `/metrics`:

```yaml
# ServiceMonitor for Prometheus
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: sol9-metrics
  namespace: sol9
spec:
  selector:
    matchLabels:
      app: bookings-api
  endpoints:
  - port: http
    path: /metrics
```

### Logging

Use centralized logging (e.g., ELK stack, Loki):

```yaml
# Fluent Bit DaemonSet for log collection
apiVersion: apps/v1
kind: DaemonSet
metadata:
  name: fluent-bit
  namespace: sol9
spec:
  # ... Fluent Bit configuration
```

## Scaling

### Horizontal Pod Autoscaling

When using a standard Deployment, reference `Deployment`; when using Argo Rollouts, reference `Rollout` and `argoproj.io/v1alpha1`:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: bookings-api-hpa
  namespace: sol9
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: bookings-api
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## Database Migrations

Run migrations as init containers or jobs:

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: bookings-migration
  namespace: sol9
spec:
  template:
    spec:
      containers:
      - name: migrate
        image: sol9/bookings-api:latest
        command: ["dotnet", "ef", "database", "update"]
        env:
        - name: ConnectionStrings__Bookings
          value: "Host=postgres;Database=bookings;..."
      restartPolicy: Never
```

## CI/CD

The repository includes a GitHub Actions workflow (`.github/workflows/ci-cd.yml`) that:

1. **Build and test** – Restore, build, and run tests with coverage.
2. **Publish** – Publish API artifacts (on `main`/`master`).
3. **Docker build and push** – Build and push images to GHCR (on `main`/`master`):
   - `ghcr.io/<owner>/sol9-bookings-api:<sha>` and `latest`
   - `ghcr.io/<owner>/sol9-orders-api:<sha>` and `latest`
   - `ghcr.io/<owner>/sol9-gateway-api:<sha>` and `latest`
4. **Deploy** (optional) – When secret `KUBE_CONFIG_DATA` (base64 kubeconfig) is set, runs `helm upgrade --install` for the three charts and optionally promotes Argo Rollouts.

To enable deploy, add a repository secret `KUBE_CONFIG_DATA` with the base64-encoded kubeconfig for your cluster.

---

## Automating Blue-Green and Kubernetes Deployment

### Automating blue-green deployment

Blue-green can be fully automated. You already use **Argo Rollouts** in Helm (blue-green strategy). Automation options:

| Approach | What is automated | How |
|----------|-------------------|-----|
| **CI-driven** | Build → push image → deploy new version (green) → optionally promote | In GitHub Actions: after `helm upgrade` with the new tag, run `kubectl argo rollouts promote` (removes the manual gate), or use **post-deploy analysis** (e.g. Argo Rollouts + Prometheus) and **auto-promote** when metrics pass. |
| **GitOps (Argo CD)** | “What’s in Git” is deployed and kept in sync; Rollouts still perform blue-green. | You change **Git** (e.g. image tag in `values.yaml` or a Kustomize overlay); Argo CD applies that to the cluster. Rollouts still create green; you then promote (manually or via Analysis + auto-promote). |

To automate promotion in CI: ensure the deploy job runs `kubectl argo rollouts promote` for each rollout after `helm upgrade`, or enable Argo Rollouts **analysis** (e.g. Prometheus success rate) and set `autoPromotionEnabled: true` (or use `autoPromotionSeconds`) in your Helm values so green is promoted automatically when analysis succeeds.

### Automating the Kubernetes process

The “Kubernetes process” (applying manifests, upgrading releases) can be automated in two main ways:

1. **CI-driven (what you have now)**  
   The pipeline builds images, pushes to a registry, then runs `helm upgrade` (and optionally promote). Every merge to `main` triggers a deploy; no manual `kubectl` or `helm` needed.

2. **GitOps (Argo CD)**  
   You only change **Git** (manifests, Helm values, or Kustomize). Argo CD watches the repo and **continuously syncs** the cluster to that state. The “Kubernetes process” is automated by Argo CD applying whatever is in Git.

Both approaches automate deployment; the difference is whether the source of truth is “CI run” or “Git state.”

---

## GitOps and Argo CD

### What is GitOps?

**GitOps** means the **desired state** of the system lives in **Git** (manifests, Helm values, Kustomize). A controller (e.g. **Argo CD**) compares the cluster to that state and applies changes so the cluster matches Git. You don’t run `kubectl apply` or `helm upgrade` from CI; you push a commit and Argo CD does the rest.

### Benefits of GitOps with Argo CD

| Benefit | Description |
|---------|-------------|
| **Single source of truth** | Git is the only place that defines “what should run.” All changes go through Git; no ad-hoc edits (or they get overwritten). |
| **Audit trail** | Every change is a commit: who, when, and why (PR/commit message). Easy to answer “what was deployed when?” |
| **Easy rollback** | Rollback = revert the commit or change the image tag in Git; Argo CD syncs. No need to re-run CI or dig through pipeline history. |
| **Drift detection** | If someone edits the cluster by hand, Argo CD sees the diff and can report it or revert to Git state. |
| **Approval and safety** | Changes go through PRs and code review; merge triggers sync. Good for production and compliance. |
| **Same workflow for many environments** | Use different folders or branches (e.g. `env/prod`, `env/staging`) or Helm values files; Argo CD applications point at each. |
| **No long-lived cluster credentials in CI** | CI only pushes images and pushes to Git. Argo CD (in the cluster) needs cluster access; CI does not need `KUBE_CONFIG_DATA`. |

### GitOps vs CI-only deploy

| | **CI runs `helm upgrade`** | **GitOps with Argo CD** |
|---|----------------------------|--------------------------|
| **Who applies to the cluster?** | GitHub Actions (or other CI) | Argo CD |
| **Credentials** | CI needs cluster access (e.g. `KUBE_CONFIG_DATA`) | Only Argo CD needs cluster access; CI only needs Git + registry |
| **Source of truth** | “Last successful pipeline run” | Git (branch/folder Argo CD watches) |
| **Rollback** | Re-run pipeline with old tag or revert and re-run | Revert commit or change tag in Git; Argo CD syncs |
| **Drift** | No built-in detection | Argo CD shows diff between cluster and Git |

### Using Argo CD with this repo

1. **Install Argo CD** in your cluster (see [Argo CD docs](https://argo-cd.readthedocs.io/)).
2. **Define the desired state in Git** – e.g. a folder with Helm values (or Kustomize) that reference your charts and image tags. CI can update the image tag in that folder (e.g. after building and pushing).
3. **Create an Argo CD Application** pointing at this repo and path (e.g. `K8s/helm` or a dedicated `deploy/` folder with values).
4. Argo CD will sync the cluster to that state. Your existing **Argo Rollouts** (blue-green) still apply; promotion can remain manual or be automated via Rollouts analysis.

With this setup, CI focuses on: build → test → push images → **update Git** (e.g. image tag). Argo CD handles applying to Kubernetes and keeping the cluster in sync with Git.

---

## Troubleshooting

### Check Pod Status

```bash
kubectl get pods -n sol9
kubectl describe pod <pod-name> -n sol9
kubectl logs <pod-name> -n sol9
```

### Check Services

```bash
kubectl get services -n sol9
kubectl get endpoints -n sol9
```

### Argo Rollouts Status

```bash
kubectl argo rollouts list -n sol9
kubectl argo rollouts status <rollout-name> -n sol9
```

### Database Connection Issues

```bash
# Check PostgreSQL
kubectl exec -it postgres-0 -n sol9 -- psql -U postgres

# Test connection from pod
kubectl exec -it bookings-api-xxx -n sol9 -- \
  dotnet ef database update --connection "Host=postgres;..."
```

## See Also

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [Argo Rollouts](https://argoproj.github.io/rollouts/)
- [Argo CD](https://argo-cd.readthedocs.io/) – GitOps and continuous sync from Git
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
