# Deployment Guide

This guide covers deploying Sol9 applications to various environments using Docker and Kubernetes.

## Prerequisites

- Docker Desktop (for local development)
- Kubernetes cluster (for production)
- kubectl configured
- Docker registry access (for container images)

## Docker Deployment

### Building Images

```bash
# Build all images
docker build -t sol9/bookings-api:latest -f Bookings.API/Dockerfile .
docker build -t sol9/orders-api:latest -f Orders.API/Dockerfile .
docker build -t sol9/gateway-api:latest -f Gateway.API/Dockerfile .

# Or use docker-compose
docker-compose build
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

### ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: sol9-config
  namespace: sol9
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  TransponderSettings__LocalBaseAddress: "http://bookings-api:8080"
  TransponderSettings__RemoteBaseAddress: "http://orders-api:8080"
```

### Secrets

```bash
# Create secrets
kubectl create secret generic sol9-secrets \
  --from-literal=postgres-password=your-password \
  --from-literal=redis-password=your-password \
  -n sol9
```

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

### GitHub Actions Example

```yaml
name: Deploy to Kubernetes

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Build and push images
      run: |
        docker build -t registry.example.com/sol9/bookings-api:${{ github.sha }} .
        docker push registry.example.com/sol9/bookings-api:${{ github.sha }}
    
    - name: Deploy to Kubernetes
      run: |
        kubectl set image deployment/bookings-api \
          bookings-api=registry.example.com/sol9/bookings-api:${{ github.sha }} \
          -n sol9
```

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
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
