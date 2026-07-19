# Getting Started

This guide covers the one-time setup required before running PowerDashboard: downloading power data and training ML models.

## Prerequisites

You need Python to run the setup scripts. Choose one of the following options:

* **[uv](https://docs.astral.sh/uv/)** (recommended) - Fast Python package installer
* **[Python](https://www.python.org/downloads/) + pip** - Standard Python installation

## 1. Set up Python Environments

### Option 1: Using `uv` (Recommended)

```bash
# Install dependencies in root folder for download script
uv sync

cd ml_experiments

# Install dependencies for model training script
uv sync
# Install folder package
uv pip install -e .
```

### Option 2: Using `pip`

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

## 2. Download the Data

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

## 3. Train the Models

```bash
cd ml_experiments

# Activate virutal environment (Powershell)
.venv\Scripts\activate
# Bash
source .venv/bin/activate

# Run training script
python scripts/train.py

# Copy models folder to inferenceservice (Powershell)
robocopy models ..\services\inferenceservice\models /E
# Bash
cp -r models/. ../services/inferenceservice/models
```

## Next steps

Once data download and model training are completed, proceed with your preferred approach:

- 🐳 **[Docker Setup](docker-setup.md)** (recommended)
- 💻 **[Local Development](local-development.md)**
- ☸️ **[Kubernetes](kubernetes.md)** 

[← Back to Main README](../README.md)