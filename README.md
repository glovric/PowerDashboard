# PowerDashboard

Dashboard web application which displays power load data of European countries.

## Architecture

The application is composed of multiple services, each having a separate responsibility:

| Component | Technology | Responsibility |
|-----------|------------|----------------|
| **Frontend** | Vue 3 | User interface |
| **Reverse Proxy** | Nginx | Routes requests to backend services |
| **Database** | PostgreSQL | Stores data and auth related tables |
| **Power Service** | .NET 10.0 | Data management and business logic |
| **Auth Service** | .NET 10.0 | Authentication and authorization |
| **Inference Service** | FastAPI | Machine learning inference |

## Getting Started

Before running the application with **Docker** or in **local development mode** download the required power data and train the ML models. These setup scripts require Python.

### Prerequisites

* **[uv](https://docs.astral.sh/uv/)** (recommended), or
* **[Python](https://www.python.org/downloads/) + pip**

### 1. Set up the Python environments

#### Option 1: Using `uv` (recommended)

```bash
# Install dependencies in root folder for download script
uv sync

cd ml_experiments

# Install dependencies for model training script
uv sync
# Install folder package
uv pip install -e .
```

#### Option 2: Using `pip`

```bash
# Create and activate virtual environment in the project root
python -m venv .venv

# PowerShell
.\.venv\Scripts\Activate.ps1
# Bash
source .venv/bin/activate

# Install dependencies for the data download script
pip install -r requirements.txt
deactivate

cd ml_experiments

# Create and activate virtual environment in ml_experiments
python -m venv .venv

# PowerShell
.\.venv\Scripts\Activate.ps1
# Bash
source .venv/bin/activate

# Install dependencies in ml_experiments for the model training script
pip install -r requirements.txt

# Install the package in editable mode
pip install -e .
```

### 2. Download the Data

After installing the Python dependencies, run the data download script.

**PowerShell**

```powershell
.\data\download_data.ps1
```

**Bash**

```bash
chmod +x ./data/download_data.sh
./data/download_data.sh
```

### 3. Train the models

```bash
cd ml_experiments

# Activate virutal environment
.venv\Scripts\activate

# Run training script
python scripts/train.py

# Copy models folder to inferenceservice (Powershell)
robocopy models ..\services\inferenceservice\models /E
# Bash
cp -r models/. ../services/inferenceservice/
```

Once data download and model training are completed, continue with either the **Docker Setup** or **Local Development** section below.

## Docker Setup

The recommended way to run the application is with **Docker Desktop** and **Docker Compose**, which starts all required services automatically.

### Prerequisites

* **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** (includes Docker Compose)


### 1. Create `.env` file

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

You can adjust the values to your liking, just make sure the entry `ConnectionStrings__DefaultConnection` has matching values with `pguser`, `pgpass` and `pgdb`.

### 2. Apply database migrations

From project root run the following commands:

```bash
docker compose --profile migration up powerservice-migrate
docker compose --profile migration up authservice-migrate
```
This step creates Postgres database container and inserts table schemas necessary for services.

### 3. Initiate database seeding

From project root run the following commands:

```bash
docker compose --profile seed up powerservice-seed
docker compose --profile seed up authservice-seed
```
This step seeds the database with power load data and an admin user.

### 4. Run the application

From project root run the following command:

```bash
docker compose up
```

This step creates and runs all the application containers.


### 5. Access the application

The application is available at **http://localhost:8080** by default. Admin dashboard is available at **http://localhost:8080/Admin**.

If port **8080** is already in use, you can change the host port by modifying the `ports` mapping for the `gateway` service in `docker-compose.yml`:

```yaml
gateway:
  ports:
    - "8080:80"
```

### 6. Stop the application

To stop all running containers:

```bash
docker compose down
```

## Kubernetes

To run the application in a local Kubernetes cluster read `k8s/README.md`.

## Local Development

To run the application in **local development mode**, install the required development tools for each component.

### Prerequisites

| Component | Prerequisite |
|-----------|--------------|
| **Frontend**     | [Node.js (npm)](https://nodejs.org/) or [Bun](https://bun.sh/)|
| **Database** | [PostgreSQL](https://www.postgresql.org/download/) |
| **Data Service** | [.NET 10.0 SDK](https://dotnet.microsoft.com/download) |
| **Auth Service** | [.NET 10.0 SDK](https://dotnet.microsoft.com/download) |
| **ML Service**   | [Python (pip)](https://www.python.org/) or [uv](https://docs.astral.sh/uv/) |

### 1. Create `.env` file

In `services/Shared` folder create `.env` file with the following entries:

```.env
FrontJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_FRONTEND
ServiceJwtSettings__Key=VERY_LONG_SECRET_KEY_AT_LEAST_32_CHARS_SERVICE
ServiceAPIKeys__InferenceService=your-very-long-secret-api-key-here-64+chars
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres
SuperUserSettings__Password=admin
ASPNETCORE_ENVIRONMENT=Development
```

Make sure to properly set the ConnectionStrings property depending on your local Postgre configuration.

### 2. Start each component in a separate terminal

Read each component's `README.md` in its respective folder. There should be 4 terminal sessions running (frontend, powerservice, authservice, inferenceservice). 

### 3. Access the application

Once all services are running, open the application in your browser at **http://localhost:8080**.