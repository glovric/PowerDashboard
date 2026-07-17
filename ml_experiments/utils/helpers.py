from pathlib import Path
from itertools import product
from typing import Literal
import json
import numpy as np
import pandas as pd
from sklearn.model_selection import TimeSeriesSplit, train_test_split
from sklearn.metrics import mean_absolute_percentage_error, r2_score, root_mean_squared_error
import xgboost as xgb
import optuna
from utils.model_config import NOWCAST_CONFIG, FORECAST_CONFIG

def add_cyclical_encoding(df: pd.DataFrame, interval: Literal[15, 60]) -> pd.DataFrame:
    """
    Adds time features to DataFrame. DataFrame index must be set to datetime.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to add features to.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with cyclical features
    """

    df = df.copy()

    if interval == 15:
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

def add_lag_roll_features(df: pd.DataFrame, target: str, interval: Literal[15, 60], model_type: Literal["nowcast", "forecast"]) -> pd.DataFrame:
    """
    Adds lag and rolling features based on target variable to DataFrame.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to add features to.

    target : str
        Target variable used to calculate lag and roll features.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    model_type : Literal["nowcast", "forecast"]
        String indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with lag and rolling features.
    """

    multiplier = 4 if interval == 15 else 1

    if model_type == "forecast":
        df[f'{target}_lag_0'] = df[target]
    
    for lag in [1, 2, 3, 6, 12, 24, 48, 72, 168]:
        df[f'{target}_lag_{lag}'] = df[target].shift(lag * multiplier)

    for w in [24, 168]:
        window_size = w * multiplier

        if model_type == "nowcast":
            target_series = df[target].shift(1)
        else:
            target_series = df[target]
    
        min_p = int(window_size * 0.5) 
        
        df[f'{target}_roll_mean_{w}'] = target_series.rolling(window_size, min_periods=min_p).mean()
        df[f'{target}_roll_std_{w}']  = target_series.rolling(window_size, min_periods=min_p).std()

    df['ramp_1h'] = target_series.diff(1 * multiplier)
    df['ramp_24h'] = target_series.diff(24 * multiplier)

    return df

def add_load_features(df: pd.DataFrame, target: str, interval: Literal[15, 60], model_type: Literal["nowcast", "forecast"]) -> pd.DataFrame:
    """
    Adds calendar, lag and rolling features to DataFrame.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to add features to.

    target : str
        Target variable used to calculate lag and roll features.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    model_type : Literal["nowcast", "forecast"]
        String indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with all input features needed for model training.
    """
    df = add_cyclical_encoding(df, interval)
    df = add_lag_roll_features(df, target, interval, model_type)
    return df

def remove_outliers(df: pd.DataFrame, target: str) -> pd.DataFrame:
    """
    Removes upper target outliers from DataFrame.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame from which outliers are removed.

    target : str
        Target variable column from which outliers are removed.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with removed outliers.    
    """

    Q3 = df[target].quantile(0.75) # Extract third quartile
    upper_bound = 2.5*Q3 # Set custom upper bound
    mask = (df[target] > upper_bound) # Extract rows bigger than bound

    if mask.sum() > 0:
        df = df[~mask] # Remove outliers

    return df

def remove_nan_targets(df: pd.DataFrame, target: str) -> pd.DataFrame:
    """
    Removes NaN target values from DataFrame.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame from which outliers are removed.

    target : str
        Target variable column from which NaNs are removed.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with NaN values.
    """
    mask_valid_target = df[target].notna() # Extract rows where target is valid (not null)
    df = df[mask_valid_target].copy() # Take into DataFrame only rows with valid target
    return df

def preprocess_data(df: pd.DataFrame, target: str, interval: Literal[15, 60], model_type: Literal["nowcast", "forecast"], horizon: Literal[6, 12, 24] = None) -> tuple[np.ndarray, np.ndarray]:
    """
    Preprocesses given DataFrame by removing outliers, adding feature columns and splitting into input (X) and target (y) data for XGBoost.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame to preprocess.

    target : str
        Target variable for model training.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    model_type : Literal["nowcast", "forecast"]
        String indicating which type of model is used for inference. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    horizon : Literal[6, 12, 24], default=None
        Integer specifying number of hours to predict into the future.

    Returns
    -------
    X, y : tuple[np.ndarray, np.ndarray]
        Input and target arrays ready for model ingestion.
    """

    df = remove_outliers(df, target) # Remove outliers from df from target column
    df = add_load_features(df, target, interval, model_type) # Create feature columns (calendar, lag) in df based on target column

    if model_type == "nowcast":
        df = remove_nan_targets(df, target)
        X = df.drop(target, axis=1).to_numpy() # Remove just the target column from input data
        y = df[target].to_numpy() # Extract target column
        return X, y
    elif model_type == "forecast":
        multiplier = 4 if interval == 15 else 1 # Multiplier to properly scale 15min data
        df["target_forecast"] = df[target].shift(-1 * horizon * multiplier) # Create forecast target variable
        df = remove_nan_targets(df, "target_forecast") # Remove NaNs from target variable column
        X = df.drop([target, "target_forecast"], axis=1).to_numpy() # Drop original target and target_forecast columns
        y = df["target_forecast"].to_numpy()
        return X, y

def load_dataset(path: Path) -> pd.DataFrame:
    """
    Loads CSV file from provided path into a DataFrame and sets the timestamp column to index.

    Parameters
    ----------
    path : Path
        CSV file path.

    Returns
    -------
    df : pd.DataFrame
        DataFrame with datetime index column.
    """
    df = pd.read_csv(path) # Read csv file from path
    df["utc_timestamp"] = pd.to_datetime(df["utc_timestamp"], utc=True) # Convert timestamp column from string to datetime
    df = df.set_index("utc_timestamp")
    return df

def train_nowcast_models(df: pd.DataFrame, interval: Literal[15, 60], models_folder: Path) -> None:
    """
    Trains nowcast models for all countries in the given DataFrame and saves them to file.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame containing different countries power load values.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    models_folder : Path
        Folder path where models are saved.

    Returns
    -------
    None
    """

    countries = set([c[:2].upper() for c in df.columns if "timestamp" not in c]) # Extract countries from column names
    multiplier = 4 if interval == 15 else 1 # Multiplier to properly scale 15min data

    for country in countries:

        # Data preparation
        target = f"{country}_load_actual_entsoe_transparency" # Build target variable column name
        df_c = pd.DataFrame(df[target]) # Create DataFrame consisting only of timestamps and current country load values
        X, y = preprocess_data(df_c, target, interval, model_type="nowcast")

        gap = 168 * multiplier # 1 week gap between train and validation sets, max lag feature is 168
        val_size = 168 * multiplier # 1 week of validation data (maybe increase to 2/3 months)

        X_train_full, X_test, y_train_full, y_test = train_test_split(X, y, test_size=0.2, shuffle=False)
        X_val, y_val = X_train_full[-val_size:], y_train_full[-val_size:] # Extract last section of X_train_full to validation set
        X_train, y_train = X_train_full[:-val_size-gap], y_train_full[:-val_size-gap]

        # Model training
        params = NOWCAST_CONFIG[interval] # Get hyperparameters for current nowcast model type
        model_c = xgb.XGBRegressor(**params)
        print(f'Training {country} | {interval} nowcast model.')
        model_c.fit(X_train, y_train, eval_set=[(X_val, y_val)], verbose=False)

        # Calculate test metrics
        y_pred = model_c.predict(X_test)
        rmse = root_mean_squared_error(y_test, y_pred)
        mape = mean_absolute_percentage_error(y_test, y_pred)*100
        r2 = r2_score(y_test, y_pred)

        print(f'{country} | {interval} nowcast model test metrics : [RMSE: {rmse}, MAPE: {mape:.3f}%, R2: {r2:.4f}, n_estimators: {model_c.best_iteration}]')

        # Save model
        path = models_folder / country / str(interval) / "nowcast"
        path.mkdir(parents=True, exist_ok=True)
        model_c.save_model(path / "xgb.ubj")

def train_forecast_models(df, interval: Literal[15, 60], models_folder: Path) -> None:
    """
    Trains forecast models for all countries in the given DataFrame and saves them to file.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame containing different countries power load values.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    models_folder : Path
        Folder path where models are saved.

    Returns
    -------
    None
    """
    countries = set([c[:2].upper() for c in df.columns if "timestamp" not in c]) # Extract countries from column names

    horizons = [6, 12, 24] # Forecast horizons (+6h, +12h, +24h)
    products = product(horizons, countries) # Create all combinations of horizons and countries
    multiplier = 4 if interval == 15 else 1 # Multiplier to properly scale 15min data

    for horizon, country in products:

        # Data preparation
        target = f"{country}_load_actual_entsoe_transparency" # Build target variable column name
        df_c = pd.DataFrame(df[target]) # Create DataFrame consisting only of timestamps and current country load values
        X, y = preprocess_data(df_c, target, interval, model_type="forecast", horizon=horizon)

        gap = 168 * multiplier # 1 week gap between train and validation sets, max lag feature is 168
        val_size = 168 * multiplier # 1 week of validation data (maybe increase to 2/3 months)

        X_train_full, X_test, y_train_full, y_test = train_test_split(X, y, test_size=0.2, shuffle=False)
        X_val, y_val = X_train_full[-val_size:], y_train_full[-val_size:] # Extract last section of X_train_full to validation set
        X_train, y_train = X_train_full[:-val_size-gap], y_train_full[:-val_size-gap]

        # Model training
        params = FORECAST_CONFIG[interval][horizon] # Get hyperparameters for current forecast model type
        model_c = xgb.XGBRegressor(**params)
        print(f'Training {country} | {interval} | {horizon} forecast model.')
        model_c.fit(X_train, y_train, eval_set=[(X_val, y_val)], verbose=False)

        # Calculate test metrics
        y_pred = model_c.predict(X_test)
        rmse = root_mean_squared_error(y_test, y_pred)
        mape = mean_absolute_percentage_error(y_test, y_pred)*100
        r2 = r2_score(y_test, y_pred)
        print(f'{country} | {interval} | {horizon} forecast model test metrics : [RMSE: {rmse}, MAPE: {mape:.3f}%, R2: {r2:.4f}, n_estimators: {model_c.best_iteration}]')

        # Save model
        path = models_folder / country / str(interval) / "forecast" / str(horizon)
        path.mkdir(parents=True, exist_ok=True)
        model_c.save_model(path / "xgb.ubj")

def search_hyperparams(df: pd.DataFrame, interval: Literal[15, 60], model_type: Literal["nowcast", "forecast"], horizon: Literal[6, 12, 24] = None, params_path: Path = "") -> None:
    """
    Performs Bayesian hyperparameter search for the provided dataset and model type. Optimal hyperparameter config is saved to file.

    Parameters
    ----------
    df : pd.DataFrame
        DataFrame containing power load values.

    interval : Literal[15, 60]
        Integer specifying time interval between data timestamps.

    model_type : Literal["nowcast", "forecast"]
        String indicating which type of model is used in search. 
        Nowcast model uses shifted target variable to exclude target variable at timestep t.

    horizon : Literal[6, 12, 24], default=None
        Integer specifying number of hours to predict into the future.

    params_path : Path, default=""
        Folder path where optimal hyperparameter configuration is saved.

    Returns
    -------
    None
    """

    multiplier = 4 if interval == 15 else 1 # Multiplier to properly scale 15min data

    TARGET = 'DE_load_actual_entsoe_transparency' # Used as baseline dataset because it has least NaNs
    df = df[[TARGET]] # Extract only target column

    X, y = preprocess_data(df, TARGET, interval, model_type, horizon) # Create input and target data for model

    print(f'X: {X.shape}, y: {y.shape}')

    X_train, _, y_train, _ = train_test_split(X, y, test_size=0.2, shuffle=False)

    def xgb_objective(trial):
        
        # Define hyperparameter search space
        params = {
            'max_depth': trial.suggest_int('max_depth', 5, 10),
            'learning_rate': trial.suggest_float('learning_rate', 0.03, 0.1, log=True), # Lr of 0.01 creates many trees, makes everything slow (but more precise)
            'subsample': trial.suggest_float('subsample', 0.5, 1.0, step=0.05),
            'colsample_bytree': trial.suggest_float('colsample_bytree', 0.5, 1.0, step=0.05),
            'min_child_weight': trial.suggest_int('min_child_weight', 3, 10),
            'gamma': trial.suggest_float('gamma', 0, 5, step=0.5),
            'reg_lambda': trial.suggest_float('reg_lambda', 1, 10, step=0.5), # L2 regularization
            'reg_alpha': trial.suggest_float('reg_alpha', 1, 10, step=0.5),  # L1 regularization
        }
        
        model = xgb.XGBRegressor(
            **params,
            random_state=42,
            n_jobs=-1,
            tree_method='hist',
            eval_metric='rmse',
            booster='gbtree',
            objective='reg:squarederror',
            n_estimators=3000, # Use at most 3000 trees (5000+ makes training and tuning slow)
            early_stopping_rounds=50, # After 50 rounds of no improvement in score stop training
            device="cuda" # Use GPU
        )
        
        # Time-series cross-validation
        tss = TimeSeriesSplit(
            n_splits=3, # Train data is approx 4 years, split into 3 years + 1 year validation
            test_size=24*365*multiplier, # 1 year of validation data
            gap=168*multiplier # Gap between train and validation, maximum lag feature is 168h
        )
        
        scores = [] # Create empty list for RMSE scores
        best_iters = [] # Create empty list for model n_estimators

        for train_idx, val_idx in tss.split(X_train): # Split each fold into train and validation sets
            X_tr, X_val = X_train[train_idx], X_train[val_idx]
            y_tr, y_val = y_train[train_idx], y_train[val_idx]
            
            model.fit(
                X_tr, y_tr, # Train on X_tr
                eval_set=[(X_val, y_val)], # X_val is unseen data, used for early stopping
                verbose=False
            )
            
            y_pred = model.predict(X_val) # Predict using current fold validation set
            rmse = root_mean_squared_error(y_val, y_pred) # Calculate current fold RMSE
            scores.append(rmse) # Save current fold RMSE
            best_iters.append(model.best_iteration) # Save current fold n_estimators
        
        mean_rmse = np.mean(scores) # Calculate mean RMSE from scores list

        print(
            f"\nTrial {trial.number}:\n"
            f"RMSE={mean_rmse:.5f}, \n"
            f"best_iterations={best_iters}, \n"
        )

        return mean_rmse # Objective is to minimize mean RMSE

    # Run optimization
    study = optuna.create_study(
        direction='minimize',
        study_name='xgb_hyperparam_optimization'
    )

    study.optimize(
        xgb_objective,
        n_trials=50, # Run 50 trials
        n_jobs=1,
        show_progress_bar=True
    )

    # Get best parameters
    best_params = study.best_params
    print(f"Best RMSE: {study.best_value:.4f}")
    print(f"Best params: {best_params}")

    # Save best parameters to JSON
    with open(params_path, "w") as f:
        json.dump(best_params, f, indent=4)