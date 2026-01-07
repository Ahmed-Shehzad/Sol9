# Sol9 GitOps Repository

This folder is the GitOps layer for Sol9. In a real setup, it lives in its own
repo (for example `sol9-gitops`). For now, it is colocated so you can learn the
flow end-to-end.

## Layout
- `argocd/`: AppProject + app-of-apps entrypoints.
- `apps/dev/`: dev environment apps (Helm Applications).
- `apps/staging/`: staging environment apps (Helm Applications).
- `apps/prod/`: prod environment apps (Helm Applications).

## Quick start
1. Update the repo URL in:
   - `K8s/gitops/argocd/project.yaml`
   - `K8s/gitops/argocd/app-of-apps.yaml`
   - `K8s/gitops/apps/dev/*.yaml`
2. Apply the GitOps bootstrap (dev + staging + prod app-of-apps):
   ```bash
   kubectl apply -n argocd -f K8s/gitops/argocd/
   ```
3. In Argo CD UI or CLI, sync `sol9-apps`.

## Notes
- `sol9-apps` is the app-of-apps. It reads `K8s/gitops/apps/dev` and creates the
  per-service Applications.
- The services deploy into the `sol9` namespace.
- Postgres and Redis are deployed once as shared infrastructure. Orders/Bookings
  both point at the shared `app` database by default.

## Split into a real GitOps repo (recommended)
1. Create a new repo (for example `sol9-gitops`).
2. Move `K8s/gitops` into the root of that repo.
3. Update `repoURL` in the YAML files to point at the new repo.
4. Re-apply `K8s/gitops/argocd/`.
