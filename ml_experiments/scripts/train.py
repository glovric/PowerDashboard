from pathlib import Path
from utils.helpers import load_dataset, train_nowcast_models, train_forecast_models

if __name__ == '__main__':

    project_root = Path(__file__).resolve().parent.parent.parent
    path60 = project_root / "data/time_series_60.csv"
    path15 = project_root / "data/time_series_15.csv"

    script_dir = Path(__file__).resolve().parent
    model_base_path = script_dir.parent / "models" # Save models to ../models

    df60 = load_dataset(path60)
    df15 = load_dataset(path15)

    train_nowcast_models(df15, interval=15, models_folder=model_base_path)
    train_nowcast_models(df60, interval=60, models_folder=model_base_path)
    
    train_forecast_models(df15, interval=15, models_folder=model_base_path)
    train_forecast_models(df60, interval=60, models_folder=model_base_path)