# ML Experiments

This folder contains power load data analysis, preprocessing and XGBoost model development. It also includes two scripts for training the models and model hyperparameter search.

## Requirements

- **[uv](https://docs.astral.sh/uv/)** or **[Python + pip](https://www.python.org/downloads/)**

## Project setup (uv)
```
uv sync
uv pip install -e .
```

## Project setup (pip)

### 1. Create virtual environment
```
python -m venv .venv
```

### 2. Activate virtual environment:

- PowerShell
```powershell
.\.venv\Scripts\Activate.ps1
```

- Bash
```bash
source .venv/bin/activate
```

### 3. Install dependencies:
```
pip install -r requirements.txt
```

## Running scripts

### 1. train.py

To train all 152 models and save them to file run
```bash
python scripts/train.py
```

Once all the models are trained they can be copied to `inferenceservice` folder.

### 2. hyperparam_search.py


To perform hyperparameter search for a specific XGBoost configuration run
```bash
python scripts/hyperparameter_search.py
```

The script saves optimal hyperparameters to `params` folder.

***

## Clear cache
```
uv cache clean
pip cache purge
```