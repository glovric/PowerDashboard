# Authentication Service

This service provides frontend and inference service with JWT tokens. The service is also responsible for user management using ASP.NET Core Identity and provides an admin dashboard.

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

### Restore .NET tools (optional, for migrations)

```bash
dotnet tool restore
```

### Build service

```bash
dotnet build
```

### Apply DB migrations

```bash
dotnet run --migrate-only
```

### Seed admin user

```bash
dotnet run --seed-only
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
docker compose --profile migration up authservice-migrate
```

### 2. Seed data

After database schema has been applied admin user can be seeded.

```bash
docker compose --profile seed up authservice-seed
```

### 3. Run service

```bash
docker compose up authservice
```