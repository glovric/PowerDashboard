# Services

- `authservice` - Authentication service for issuing tokens

- `powerservice` - Data service which provides frontend and inference service with power load data

- `inferenceservice` - Service for running inference on an ML model

- `Shared` - settings folder used by all services

## Setup

⚠️ **Important**  ⚠️

Before the services are run make sure all the configuration is set depending on local or production mode.

### Local development

For local development files `.env`, `sharedsettings.json` and `sharedsettings.Development.json` are needed in the `Shared` folder.

#### `Shared/sharedsettings.Development.json` example

```json
{
  "FrontJwtSettings": {
    "Issuer": "http://localhost:5253",
    "Audience": "http://localhost:5169"
  },
  "ServiceJwtSettings": {
    "Issuer": "http://localhost:5253",
    "Audience": "http://localhost:5169"
  },
  "Services": {
    "AuthService": "http://localhost:5253",
    "PowerService": "http://localhost:5169"
  },
  "SuperUserSettings": {
    "UserName": "admin",
    "Email": "admin@admin.com"
  },
  "AllowedOrigins": [
    "http://localhost:8080" 
  ],
  "DataFiles": {
    "TimeSeries60": "../../data/time_series_60.csv",
    "TimeSeries15": "../../data/time_series_15.csv"
  }
}
```

#### `Shared/.env` example

```env
FrontJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_FRONTEND
ServiceJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_SERVICE
ServiceAPIKeys__InferenceService=your-very-long-secret-api-key-here-64+chars
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres
SuperUserSettings__Password=admin
ASPNETCORE_ENVIRONMENT=Development
```

### Production (Docker or Kubernetes)

For production environment (both Docker and Kubernetes) files `sharedsettings.json` and `sharedsettings.Production.json` are needed in the `Shared` folder. For Docker the `.env` should be placed in project root, and for Kubernetes the file `secrets.yaml` should be placed in `k8s` folder.

#### `Shared/sharedsettings.Production.json` example

```json
{
  "FrontJwtSettings": {
    "Issuer": "https://api.myapp.com",
    "Audience": "https://app.myapp.com"
  },
  "ServiceJwtSettings": {
    "Issuer": "https://auth.myapp.com",
    "Audience": "internal-services"
  },
  "Services": {
    "AuthService": "http://authservice:8080",
    "PowerService": "http://powerservice:8080"  
  },
  "SuperUserSettings": {
    "UserName": "admin",
    "Email": "admin@admin.com"
  },
  "DataFiles": {
    "TimeSeries60": "/data/time_series_60.csv",
    "TimeSeries15": "/data/time_series_15.csv"
  }
}
```

#### `.env` example for Docker (in project root)

```env
pguser=myuser
pgpass=mypassword
pgdb=mydatabase

FrontJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_FRONTEND
ServiceJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_SERVICE
ServiceAPIKeys__InferenceService=your-very-long-secret-api-key-here-64+chars
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword
SuperUserSettings__Password=admin
ASPNETCORE_ENVIRONMENT=Production
```

#### `k8s/secrets.yaml` example for Kubernetes

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

## Settings details

- `FrontJwtSettings` section refers to JWT access token provided to frontend

- `ServiceJwtSettings` section refers to JWT access token provided to inference service

- `Services` section refers to URLs used by the inference service to ping other services

- `ServiceAPIKeys` section refers to API keys for accessing auth endpoints while inference service is still unauthenticated

- `SuperUserSettings` section refers to superuser/admin credentials for admin dashboard

- `DataFiles` section refers to .csv file paths used by Power Service to seed the database

- `ConnectionStrings` section refers to PostgreSQL database credentials