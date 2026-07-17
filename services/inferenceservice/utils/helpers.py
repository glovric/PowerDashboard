import numpy as np
import pandas as pd
import datetime
import requests
import xgboost
from fastapi import HTTPException, Request, status
from fastapi.responses import Response, JSONResponse
from core.enums import Interval, ModelType, ForecastHorizon

def add_cyclical_encoding(df: pd.DataFrame, interval: Interval) -> pd.DataFrame:
    """
    Adds time features to DataFrame. DataFrame index must be set to datetime.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to add features to.

    interval : Interval
        Integer enumeration specifying time interval between data timestamps.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with cyclical features
    """
    if interval == Interval.QUARTER:
        df['minute_sin'] = np.sin(2 * np.pi * df.index.minute / 60)
        df['minute_cos'] = np.cos(2 * np.pi * df.index.minute / 60)

    df['hour_sin'] = np.sin(2 * np.pi * df.index.hour / 24)
    df['hour_cos'] = np.cos(2 * np.pi * df.index.hour / 24)

    df['dow_sin'] = np.sin(2 * np.pi * df.index.dayofweek / 7)
    df['dow_cos'] = np.cos(2 * np.pi * df.index.dayofweek / 7)

    df['is_weekend'] = (df.index.dayofweek >= 5).astype(int)

    df['month_sin'] = np.sin(2 * np.pi * (df.index.month / 12))
    df['month_cos'] = np.cos(2 * np.pi * (df.index.month / 12))

    days_in_year = df.index.is_leap_year.astype(int) + 365
    df['dayofyear_sin'] = np.sin(2 * np.pi * df.index.dayofyear / days_in_year)
    df['dayofyear_cos'] = np.cos(2 * np.pi * df.index.dayofyear / days_in_year)

    week = df.index.isocalendar().week.astype(int)
    df['weekofyear_sin'] = np.sin(2 * np.pi * week / 52)
    df['weekofyear_cos'] = np.cos(2 * np.pi * week / 52)  

    return df

def add_lag_roll_features(df: pd.DataFrame, target: str, interval: Interval, model_type: ModelType) -> pd.DataFrame:
    """
    Adds lag and roll features based on target variable to DataFrame.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to add features to.

    y : str
        Target variable used to calculate lag and roll features.

    interval : Interval
        Integer enumeration specifying time interval between data timestamps.

    model_type : ModelType
        Enumeration indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with lag and roll features.
    """
    multiplier = 4 if interval == Interval.QUARTER else 1 # Multiplier is used to scale properly between 15min and 1h data.

    if model_type == ModelType.FORECAST: # Forecast model was trained using target variable at timestep t.
        df[f'{target}_lag_0'] = df[target] # This assumes data is available at timestep t (as opposed to nowcast).
    
    for lag in [1, 2, 3, 6, 12, 24, 48, 72, 168]:
        df[f'{target}_lag_{lag}'] = df[target].shift(lag * multiplier) # Calculate lag features

    for w in [24, 168]:
        window_size = w * multiplier
        min_p = int(window_size * 0.5) 

        if model_type == ModelType.NOWCAST: # Nowcast model was trained using target variable at timestep t-1.
            base_data = df[target].shift(1) # This assumes data is available up until timestep t-1 (timestep t excluded).
        else:
            base_data = df[target]
        
        df[f'{target}_roll_mean_{w}'] = base_data.rolling(window_size, min_periods=min_p).mean() # Calculate rolling features
        df[f'{target}_roll_std_{w}']  = base_data.rolling(window_size, min_periods=min_p).std()

    df['ramp_1h'] = base_data.diff(1 * multiplier) # Calculate ramp features
    df['ramp_24h'] = base_data.diff(24 * multiplier)

    return df

def preprocess_df(df: pd.DataFrame, target: str, interval: Interval, model_type: ModelType) -> pd.DataFrame:
    """
    Preprocess DataFrame for model inference. Adds cyclical, lag and roll features.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to preprocess.

    target : str
        Target variable used to calculate lag and roll features.

    interval : Interval
        Integer enumeration specifying time interval between data timestamps.

    model_type : ModelType
        Enumeration indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    Returns
    -------
    df : pd.DataFrame
        Preprocessed DataFrame for model inference.
    """

    df["timestamp"] = pd.to_datetime(df["timestamp"], utc=True) # Convert timestamp object column to datetime
    assert df['timestamp'].dt.tz == datetime.timezone.utc # Makesure timestamp format is UTC
    df["load"] = pd.to_numeric(df["load"], errors='coerce') # Convert load object column to float
    df = df.set_index("timestamp") # Set index to timestamp column, enables cyclical feature calculation
    df = add_cyclical_encoding(df, interval) # Add cyclical features
    df = add_lag_roll_features(df, target, interval, model_type) # Add lag and roll features
    df = df.dropna() # Drop NaN rows (sketchy, if targets are mostly NaN everything gets dropped)
    return df

def convert_response_to_df(response: requests.Response, interval: Interval, model_type: ModelType) -> pd.DataFrame:
    """
    Converts an HTTP response first to JSON, then to pandas DataFrame. 
    DataFrame object is then preprocessed for model inference.

    Parameters
    ----------
    response : requests.Response
        Response object to convert. Should contain a list of power load values and a list of timestamps.

    interval : Interval
        Integer enumeration specifying time interval between data timestamps.

    model_type : ModelType
        Enumeration indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    Returns
    -------
    df : pd.DataFrame
        Preprocessed DataFrame for model inference.
    """
    response_filtered = response.json()
    response_filtered.pop("histLabels")
    df = pd.DataFrame(response_filtered) # Convert response to JSON, then to DataFrame
    df = preprocess_df(df, target="load", interval=interval, model_type=model_type)
    return df

def calculate_bin_counts(values: list, bin_labels: list[str]) -> list[int]:
    """
    Calculates histogram counts for given values based on .NET-style bin labels.

    Supported label formats:
        "< 10"
        "10 - 20"
        "> 20"

    Args:
        values: List of numeric values to bin.
        bin_labels: List of histogram bin labels.

    Returns:
        List of integer counts for each bin.
    """

    #if not values or not bin_labels:
       # return []

    bin_count = len(bin_labels)
    counts = [0] * bin_count

    def parse_number(value: str) -> float:
        return float(value.strip().replace(',', '.'))

    # Parse bins into structured ranges
    parsed_bins = []

    for i, label in enumerate(bin_labels):
        label = label.strip()

        # First bin: "< value"
        if label.startswith('<'):
            upper = parse_number(label[1:])
            parsed_bins.append({
                "type": "lt",
                "upper": upper
            })

        # Last bin: "> value"
        elif label.startswith('>'):
            lower = parse_number(label[1:])
            parsed_bins.append({
                "type": "gt",
                "lower": lower
            })

        # Middle bins: "start - end"
        else:
            parts = label.split(' - ')
            if len(parts) != 2:
                raise ValueError(f"Invalid label format: {label}")

            start = parse_number(parts[0])
            end = parse_number(parts[1])

            parsed_bins.append({
                "type": "range",
                "start": start,
                "end": end
            })

    # Bin the values
    for val in values:
        if val is None:
            continue

        for idx, bin_def in enumerate(parsed_bins):

            # "< x"
            if bin_def["type"] == "lt":
                if val < bin_def["upper"]:
                    counts[idx] += 1
                    break

            # "> x"
            elif bin_def["type"] == "gt":
                if val >= bin_def["lower"]:
                    counts[idx] += 1
                    break

            # "x - y"
            else:
                start = bin_def["start"]
                end = bin_def["end"]
                if start <= val < end:
                    counts[idx] += 1
                    break

    return counts

def model_predict_from_response(response: requests.Response, model: xgboost.XGBRegressor, interval: Interval, model_type: ModelType) -> tuple[np.ndarray, pd.Index]:
    """
    Runs model inference.

    Parameters
    ----------
    response : requests.Response
        Response object containing a list of power load values and a list of timestamps used to build input array.

    model : xgboost.XGBRegressor
        XGBoost model used to predict power load values.

    interval : Interval
        Integer enumeration specifying time interval between data timestamps.

    model_type : ModelType
        Enumeration indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    Returns
    -------
    y_pred, timestamps : tuple[np.ndarray, pd.Index]
        Tuple containing predicted power load values and timestamps.
    """
    df = convert_response_to_df(response, interval, model_type)

    X = df.drop("load", axis=1).to_numpy() # Drop target variable, convert DataFrame to np.ndarray
    y_pred = model.predict(X) # Run model inference

    bin_labels = response.json()['histLabels']
    hist_data = calculate_bin_counts(y_pred, bin_labels)
    ramp_data = pd.Series(y_pred).diff().dropna().to_numpy()

    return y_pred, hist_data, ramp_data

def create_forecast_timestamps(forecast_date: str, interval: Interval, horizon: ForecastHorizon) -> pd.Index:
    """
    Creates timestamps range (forecast_date - horizon, forecast_date + horizon).
    This is used because fetching future timestamps from database is tricky.

    Parameters
    ----------
    forecast_date : str
        Forecast start timestamp.

    interval : Interval
        Integer enumeration specifying time interval between data timestamps.

    horizon : ForecastHorizon
        Integer enumeration specifying number of hours to predict into the future.

    Returns
    -------
    forecast_timestamps : pd.Index
        Formatted timestamps range.
    """
    frequency = "15min" if interval == Interval.QUARTER else "h" # Set frequency based on interval
    start_ts = pd.to_datetime(forecast_date, utc=True) # Convert to pd.Timestamp
    date_range = pd.date_range(
        start=start_ts - pd.Timedelta(hours=horizon.value),
        end=start_ts + pd.Timedelta(hours=horizon.value),
        inclusive='left',
        freq=frequency
    ) # Create range of timestamps where start ts is included, end ts excluded
    formatted_range = date_range.strftime("%Y-%m-%d %H:%M") # Format timestamps
    return formatted_range

async def empty_response_handler(request: Request, exc: HTTPException) -> Response:
    """
    Used to override default FastAPI response to exclude body, only status code included.
    """
    return Response(
        status_code=exc.status_code
    )