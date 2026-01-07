# Sol9 Kubernetes Learning Path

This guide continues from the current GitOps + Argo CD setup and walks through
Ingress, HPA, and blue-green deployments using this repo's charts.

## Current state (what you already have)
- Charts live in `K8s/helm/*`.
- GitOps repo: https://github.com/Ahmed-Shehzad/Sol9_K8s
- Argo CD apps are healthy for dev/staging/prod.
- Gateway service name: `sol9-gateway-api`.

## 1) Verify cluster + Argo CD health
```bash
kubectl -n argocd get applications
kubectl -n sol9 get pods
kubectl -n sol9-staging get pods
kubectl -n sol9-prod get pods
```

## 2) Ingress (NGINX)
### 2.1 Install NGINX Ingress Controller
If you don't already have it:
```bash
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
helm install ingress-nginx ingress-nginx/ingress-nginx -n ingress-nginx --create-namespace
```

### 2.2 Enable ingress for the gateway (recommended)
Edit the GitOps values for `sol9-gateway-api` in the GitOps repo and add:
```yaml
ingress:
  enabled: true
  className: nginx
  hosts:
    - host: gateway.local
      paths:
        - path: /
          pathType: Prefix
```

### 2.3 Map the host locally
```bash
sudo sh -c 'echo "127.0.0.1 gateway.local" >> /etc/hosts'
```

### 2.4 Test
```bash
curl -i http://gateway.local/orders/api/v1/orders
```

## 3) Horizontal Pod Autoscaling (HPA)
### 3.1 Install metrics-server (required by HPA)
```bash
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
```
For kind, patch insecure TLS if needed:
```bash
kubectl -n kube-system patch deployment metrics-server \
  --type='json' -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]'
```

### 3.2 Enable HPA in a chart (example: orders-api)
Update GitOps values:
```yaml
hpa:
  enabled: true
  minReplicas: 1
  maxReplicas: 3
  targetCPUUtilizationPercentage: 75
```

### 3.3 Observe
```bash
kubectl -n sol9 get hpa
kubectl -n sol9 top pods
```

## 4) Blue-Green Deployments (Argo Rollouts)
### 4.1 Install Argo Rollouts controller
```bash
kubectl apply -f https://github.com/argoproj/argo-rollouts/releases/latest/download/install.yaml
```

### 4.2 Install the kubectl plugin (recommended)
```bash
brew install argoproj/tap/kubectl-argo-rollouts
```

### 4.3 Add a Rollout to a chart (example)
Instead of `Deployment`, add a `Rollout` template in the chart:
```yaml
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: sol9-orders-api
spec:
  replicas: 2
  selector:
    matchLabels:
      app.kubernetes.io/name: orders-api
      app.kubernetes.io/instance: sol9
  template:
    metadata:
      labels:
        app.kubernetes.io/name: orders-api
        app.kubernetes.io/instance: sol9
    spec:
      containers:
        - name: orders-api
          image: ghcr.io/ahmed-shehzad/sol9-orders-api:prod
  strategy:
    blueGreen:
      activeService: sol9-orders-api
      previewService: sol9-orders-api-preview
      autoPromotionEnabled: true
```
You also need a preview Service (`sol9-orders-api-preview`).

### 4.4 Promote / rollback
```bash
kubectl argo rollouts get rollout sol9-orders-api -n sol9
kubectl argo rollouts promote sol9-orders-api -n sol9
kubectl argo rollouts undo sol9-orders-api -n sol9
```

## 5) Notes about health probes
- Liveness and readiness use `/alive` to avoid dependency loops during startup.
- `/health` includes Redis/Postgres/Transponder checks and can fail if remote
  endpoints are not ready.

## 6) Common troubleshooting
```bash
kubectl -n sol9 describe pod <pod>
kubectl -n sol9 logs <pod>
kubectl -n argocd get applications
```
