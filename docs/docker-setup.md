# Docker Setup

The recommended way to run the application is with **Docker Desktop** and **Docker Compose**, which starts all required services automatically.

## Prerequisites

* Complete the [Getting started](./getting-started.md) steps first (data download & model training)
* **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (includes Docker Compose)

## 1. Create `.env` file

In project root create `.env` file with the following entries:

```.env
# Postgres database settings
pguser=myuser
pgpass=mypassword
pgdb=mydatabase

# Services settings
FrontJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_FRONTEND
ServiceJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_SERVICE
ServiceAPIKeys__InferenceService=your-very-long-secret-api-key-here-64+chars
ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=mydatabase;Username=myuser;Password=mypassword
SuperUserSettings__Password=admin
ASPNETCORE_ENVIRONMENT=Production
```

⚠️ **Important** Make sure the entry `ConnectionStrings__DefaultConnection` has matching values with `pguser`, `pgpass` and `pgdb`.

## 2. Apply database migrations

From project root run:

```bash
docker compose --profile migration up powerservice-migrate
docker compose --profile migration up authservice-migrate
```
This step creates Postgres database container and inserts table schemas necessary for services.

## 3. Initiate database seeding

From project root run:

```bash
docker compose --profile seed up powerservice-seed
docker compose --profile seed up authservice-seed
```
This step seeds the database with power load data and an admin user.

## 4. Run the application

From project root run:

```bash
docker compose up
```

This step creates and runs all the application containers.

## 5. Access the application

The application is available at **http://localhost:8080** by default. Admin dashboard is available at **http://localhost:8080/Admin**.

### Changing the port

If port **8080** is already in use, you can change the host port by modifying the `ports` mapping for the `gateway` service in `docker-compose.yml`:

```yaml
gateway:
  ports:
    - "8080:80"
```

## 6. Stop the application

To stop all running containers:

```bash
docker compose stop
```

## Troubleshooting

- Ensure Docker Desktop is running
- Check that ports are not already in use
- Verify `.env` file configuration matches your setup

[← Back to Main README](../README.md)