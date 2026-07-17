from pathlib import Path
from typing import Literal
from utils.helpers import load_dataset, search_hyperparams

if __name__ == '__main__':

    project_root = Path(__file__).resolve().parent.parent.parent
    script_dir = Path(__file__).resolve().parent

    interval: Literal[15, 60] = 15
    model_type: Literal["nowcast", "forecast"] = "forecast"
    horizon: Literal[6, 12, 24] = 24

    # Save model hyperparams to ../params
    if model_type == "forecast":
        params_base_path = script_dir.parent / "params" / f"FORECAST_{interval}_+{horizon}.json"
    elif model_type == "nowcast":
        params_base_path = script_dir.parent / "params" / f"NOWCAST_{interval}.json"

    params_base_path.parent.mkdir(parents=True, exist_ok=True)

    path = project_root / f"data/time_series_{interval}.csv"
    df = load_dataset(path)

    search_hyperparams(df, interval, model_type, horizon, params_path=params_base_path)