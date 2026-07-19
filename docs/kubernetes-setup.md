# Local Kubernetes setup

## Prerequisites

* **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (or other Kubernetes runtime)
* **[minikube](https://minikube.sigs.k8s.io/docs/start/)**
* **[kubectl](https://kubernetes.io/docs/tasks/tools/#kubectl)** (bundled with Docker Desktop)

## 1. Create Minikube cluster

```bash
minikube start
```

## 2. Start Minikube Dashboard (optional but handy)

```bash
minikube dashboard
```

## 3. Create ConfigMaps

Run from project root:

```bash
kubectl create configmap shared-settings --from-file=sharedsettings.Production.json=services/Shared/sharedsettings.Production.json
kubectl create configmap nginx-config --from-file=default.conf=nginx/conf.d/default.conf
kubectl get configmap
kubectl describe configmap shared-settings
```

## 4. Create `secrets.yaml` in `/k8s` folder

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: app-secret
type: Opaque

stringData:
  pguser: myuser
  pgpass: mypassword
  pgdb: mydatabase

  FrontJwtSettings__Key: VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_FRONTEND
  ServiceJwtSettings__Key: VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_SERVICE
  ServiceAPIKeys__InferenceService: your-very-long-secret-api-key-here-64+chars
  ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword
  SuperUserSettings__Password: admin
  ASPNETCORE_ENVIRONMENT: Production
```

## 5. Copy seed data to Minikube

```bash
# SSH into Minikube
minikube ssh

# Create data directory inside Minikube VM
sudo mkdir -p /data

# Exit SSH
exit

# Copy files from your host to Minikube
minikube cp ./data/time_series_15.csv /data/time_series_15.csv
minikube cp ./data/time_series_60.csv /data/time_series_60.csv
```

## 6. Copy inference models to Minikube

```bash
# Create a tar file of models
tar -czf models.tar.gz -C ./services/inferenceservice models/

# Copy tar from your host to Minikube
minikube cp .\models.tar.gz /tmp/models.tar.gz

# SSH into Minikube
minikube ssh

# Extract tar file
sudo tar -xzf /tmp/models.tar.gz -C /

# Remove tar file
sudo rm /tmp/models.tar.gz

# Exit Minikube
exit
```

## 7. Build Docker Images for each component

```bash
docker build -f .\services\powerservice\Dockerfile -t powerdashboard-powerservice .\services\
docker build -f .\services\powerservice\Dockerfile -t powerdashboard-powerservice-migrate .\services\
docker build -f .\services\powerservice\Dockerfile -t powerdashboard-powerservice-seed .\services\

docker build -f .\services\authservice\Dockerfile -t powerdashboard-authservice .\services\
docker build -f .\services\authservice\Dockerfile -t powerdashboard-authservice-migrate .\services\
docker build -f .\services\authservice\Dockerfile -t powerdashboard-authservice-seed .\services\

docker build -f .\services\inferenceservice\Dockerfile -t powerdashboard-inferenceservice .\services\

docker build -f .\nginx\Dockerfile -t powerdashboard-gateway .
```

## 8. Load images into Minikube

```bash
minikube image load powerdashboard-powerservice:latest
minikube image load powerdashboard-powerservice-migrate:latest
minikube image load powerdashboard-powerservice-seed:latest

minikube image load powerdashboard-authservice:latest
minikube image load powerdashboard-authservice-migrate:latest
minikube image load powerdashboard-authservice-seed:latest

minikube image load powerdashboard-inferenceservice:latest
minikube image load powerdashboard-gateway:latest
```

## 9. Apply secrets

```bash
kubectl apply -f k8s/secrets.yaml
```

## 10. Run deployments

```bash
# Create database
kubectl apply -f k8s/postgres/

# Wait for database pod to become available
kubectl rollout status deployment/postgres

# Apply database migrations
kubectl apply -f k8s/jobs/power-migrate-job.yaml
kubectl apply -f k8s/jobs/auth-migrate-job.yaml

# Seed database
kubectl apply -f k8s/jobs/power-seed-job.yaml
kubectl apply -f k8s/jobs/auth-seed-job.yaml

# Create services
kubectl apply -f k8s/powerservice/
kubectl apply -f k8s/authservice/
kubectl apply -f k8s/inferenceservice/
kubectl apply -f k8s/gateway/
```

## 11. Make gateway URL acessible to host machine.

Open the URL in browser you see in terminal to access the app.

```bash
minikube service gateway-service --url
```

## Kubernetes overview

```bash
kubectl get pods
kubectl get pvc
kubectl get svc
```