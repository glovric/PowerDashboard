# Local Development

To run the application in **local development mode**, install the required development tools for each component.

## Prerequisites

* Complete the [Getting started](../README.md) steps first (data download & model training).

| Component | Prerequisite |
|-----------|--------------|
| **Frontend**     | [Node.js (npm)](https://nodejs.org/) or [Bun](https://bun.sh/)|
| **Database** | [PostgreSQL](https://www.postgresql.org/download/) |
| **Data Service** | [.NET 10.0 SDK](https://dotnet.microsoft.com/download) |
| **Auth Service** | [.NET 10.0 SDK](https://dotnet.microsoft.com/download) |
| **ML Service**   | [Python (pip)](https://www.python.org/) or [uv](https://docs.astral.sh/uv/) |

## 1. Configure Environment Variables

In `services/Shared` folder create `.env` file with the following entries:

```.env
FrontJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_FRONTEND
ServiceJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_SERVICE
ServiceAPIKeys__InferenceService=your-very-long-secret-api-key-here-64+chars
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres
SuperUserSettings__Password=admin
ASPNETCORE_ENVIRONMENT=Development
```

⚠️ **Important** Adjust the `ConnectionStrings__DefaultConnection` property to match your local PostgreSQL configuration.

## 2. Start each component

You will need 4 separate terminal sessions running simultaneously:

1. **Frontend** - See [frontend/README.md](../front/README.md) for instructions
2. **Powerservice** - See [frontend/README.md](../services/powerservice/README.md) for instructions
3. **Authservice** - See [frontend/README.md](../services/authservice/README.md) for instructions
4. **Inferenceservice** - See [frontend/README.md](../services/inferenceservice/README.md) for instructions

Each component's folder contains its own README with specific setup instructions.

## 3. Access the application

Once all services are running, open the application in your browser at **http://localhost:8080**.

[← Back to Main README](../README.md)