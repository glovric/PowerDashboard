# PowerDashboard

Dashboard web application which displays power load data of European countries.

## Architecture Overview

The application is composed of multiple microservices:

| Component | Technology | Responsibility |
|-----------|------------|----------------|
| **Frontend** | Vue 3 | User interface |
| **Reverse Proxy** | Nginx | Routes requests to backend services |
| **Database** | PostgreSQL | Stores data and auth related tables |
| **Power Service** | .NET 10.0 | Data management and business logic |
| **Auth Service** | .NET 10.0 | Authentication and authorization |
| **Inference Service** | FastAPI | Machine learning inference |

## Prerequisites (One-time Setup)

Before running the application, you need to download power data and train ML models. **This step is required for all setup methods**.

👉 **[Complete Getting Started Guide](docs/getting-started.md)**

## Quick Start

Choose your preferred setup method:

- 🐳 **[Docker Setup](docs/docker-setup.md)** - Recommended for production/testing
- 💻 **[Local Development](docs/local-development.md)** - For development and debugging
- ☸️ **[Kubernetes](docs/kubernetes-setup.md)** - For container orchestration

## Documentation

- [Getting Started](docs/getting-started.md) - Data download and model training
- [Docker Setup](docs/docker-setup.md) - Run with Docker Compose
- [Local Development](docs/local-development.md) - Run application locally
- [Kubernetes](docs/kubernetes-setup.md) - Deploy to local Kubernetes cluster