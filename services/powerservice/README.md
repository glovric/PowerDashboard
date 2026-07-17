# Power Service

This service provides frontend and inference service with power load data from PostgreSQL database. Endpoints are protected using JWT authorization policies.  

## Local setup

⚠️ **Important**  ⚠️

Before the service is initialized make sure all the configuration is set in Shared settings folder (`services/Shared/`). For local development files `.env`, `sharedsettings.json` and `sharedsettings.Development.json` are needed. Examine the `README.md` from project root for examples.

### Requirements

- **[.NET SDK 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)**
- **[PostgreSQL](https://www.postgresql.org/download/)**

### Restore NuGet packages

```bash
dotnet restore
```

### Restore .NET tools

```bash
dotnet tool restore
```

### Apply DB migrations

```bash
dotnet ef database update
```

### Build service

```bash
dotnet build
```

### Run service
```bash
dotnet run
```

## Other useful .NET commands 

### Create and apply migrations

```bash
dotnet ef migrations add NewMigration
dotnet ef database update --verbose
```

### Clearing cache

```bash
dotnet nuget locals --clear all
```

## Docker setup

To start this service in Docker run the following commands from project root:

### 1. Run migrations

This step initializes Postgres database and runs service database migrations.

```bash
docker compose --profile migration up powerservice-migrate
```

### 2. Seed data

After database schema has been applied power data can be seeded.

```bash
docker compose --profile seed up powerservice-seed
```

### 3. Run service

```bash
docker compose up powerservice
```