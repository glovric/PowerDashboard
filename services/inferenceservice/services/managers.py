import requests
import time
import xgboost as xgb
from fastapi import HTTPException
from core.config import settings
from core.logger import logger
from core.config import settings
from core.enums import Interval, Country, ForecastHorizon

class TokenManager:
    """
    Class for managing and retrieving JWT access tokens for intraservice communication.
    """

    def __init__(self):
        self._apikey = settings.service_api_key
        self._expiration_seconds = settings.token_expiration_seconds
        self._token = None
        self._expires_at = 0
    
    def _fetch_new_token(self) -> str:
        """
        Helper method for fetching a new JWT access token.

        Returns
        -------
        token : str
            Newly fetched JWT access token.
        """
        url = f"{settings.authservice_baseurl}/auth/getservicetoken" # Build API url

        try:
            response = requests.post(url, headers={"X-API-Key": self._apikey}) # Send request to API with apikey header
            response.raise_for_status()
            return response.json()['access_token'] # Extract access token

        except requests.exceptions.RequestException as e:
            logger.error(f"Error pinging API: {e}")

    def get_token(self) -> str:
        """
        Method for retrieving a JWT access token.

        Returns
        -------
        token : str
            JWT access token.
        """
        # Check if we have a token and if it's still valid (with 60s buffer)
        if self._token and time.time() < (self._expires_at - 60):
            return self._token
        
        # Token missing or expired: Fetch new one
        new_token = self._fetch_new_token()
        self._token = new_token
        self._expires_at = time.time() + self._expiration_seconds
        return self._token

class ModelManager:
    """
    Class for managing XGBoost regression models.
    """

    def __init__(self):
        self.nowcast_models = {}
        self.forecast_models = {}

    def _load_nowcast_model(self, key: tuple[Country, Interval]) -> None:
        """
        Helper method to load a nowcasting model into model dictionary.

        Parameters
        ----------
        key : tuple[Country, Interval]
            Tuple object containing model country and interval.

        Returns
        -------
        None
        """
        try:
            country, interval = key     # Extract country and interval from key tuple
            path = f"models/{country.value}/{interval.value}/nowcast/xgb.ubj"   # Build model path
            model = xgb.XGBRegressor()      # Create new model instance
            model.load_model(path)      # Load country model parameters
            self.nowcast_models[key] = model        # Store model in model dictionary
            logger.info(f"Nowcast model {country.value}/{interval.value} loaded successfully.")
        except Exception as e:
            logger.error(f"Nowcast model {country.value}/{interval.value} failed to load: {e}")
            raise HTTPException(status_code=404, detail=f"Forecast model {country.value}|{interval.value} failed to load.")
    
    def _load_forecast_model(self, key: tuple[Country, Interval, ForecastHorizon]) -> None:
        """
        Helper method to load a forecasting model into model dictionary.

        Parameters
        ----------
        key : tuple[Country, Interval, ForecastHorizon]
            Tuple object containing model country, interval and horizon.

        Returns
        -------
        None
        """
        try:
            country, interval, horizon = key        # Extract country, interval and horizon from key tuple
            path = f"models/{country.value}/{interval.value}/forecast/{horizon.value}/xgb.ubj"   # Build model path
            model = xgb.XGBRegressor()      # Create new model instance
            model.load_model(path)      # Load country model parameters
            self.forecast_models[key] = model       # Store model in model dictionary
            logger.info(f"Forecast model {country.value}|{interval.value}|{horizon.value} loaded successfully.")
        except Exception as e:
            logger.error(f"Forecast model {country.value}|{interval.value}|{horizon.value} failed to load: {e}")
            raise HTTPException(status_code=404, detail=f"Forecast model {country.value}|{interval.value}|{horizon.value} failed to load.")
    
    def get_forecast_model(self, country: Country, interval: Interval, horizon: ForecastHorizon) -> xgb.XGBRegressor:
        """
        Fetch forecasting model from the ModelManager.

        Parameters
        ----------
        country : Country
            Country enumeration specifier.

        interval : Interval
            Integer enumeration specifying time interval between data timestamps.

        horizon : ForecastHorizon
            Integer enumeration specifying number of hours to predict into the future.

        Returns
        -------
        forecast_model : xgb.XGBRegressor
            Forecasting model for the given country, interval and horizon.
        """
        key = (country, interval, horizon)
        if key not in self.forecast_models:
            self._load_forecast_model(key)
        return self.forecast_models.get(key)
    
    def get_nowcast_model(self, country: Country, interval: Interval) -> xgb.XGBRegressor:
        """
        Fetch nowcasting model from the ModelManager.

        Parameters
        ----------
        country : Country
            Country enumeration specifier.

        interval : Interval
            Integer enumeration specifying time interval between data timestamps.

        Returns
        -------
        nowcast_model : xgb.XGBRegressor
            Nowcasting model for the given country and interval.
        """
        key = (country, interval)
        if key not in self.nowcast_models:
            self._load_nowcast_model(key)
        return self.nowcast_models.get(key)
