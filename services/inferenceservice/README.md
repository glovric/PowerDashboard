# Inference Service

This service provides frontend with machine learning model (XGBoost) inference results.

## Local setup

⚠️ **Important**  ⚠️

Before the service is initialized make sure all the configuration is set in Shared settings folder (`services/Shared/`). For local development files `.env`, `sharedsettings.json` and `sharedsettings.Development.json` are needed. Examine the `README.md` from project root for examples.

### Requirements

- **[uv](https://docs.astral.sh/uv/)** (recommended), or
- **[Python](https://www.python.org/downloads/) + pip**

### 1. Install Python Dependencies

#### Option 1: Using `uv` (recommended)

```bash
uv sync
```

#### Option 2: Using `pip`

1. Create a virtual environment:

```bash
python -m venv .venv
```

2. Activate the virtual environment:

**PowerShell**

```powershell
.\.venv\Scripts\Activate.ps1
```

**Bash**

```bash
source .venv/bin/activate
```

3. Install the dependencies:

```bash
pip install -r requirements.txt
```

### 2. Run service

```bash
# using uv
uv run uvicorn main:app

# or using pip
uvicorn main:app
```

## Docker setup

To start this service in Docker run the following commands from project root:

### 1. Run service

```bash
docker compose up inferenceservice
```