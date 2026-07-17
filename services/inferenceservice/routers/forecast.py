from fastapi import APIRouter, Depends, Request
import requests
from utils.helpers import model_predict_from_response, create_forecast_timestamps
from schemas.requests import ForecastRequest
from core.enums import ModelType, parse_interval, parse_country, parse_forecast_horizon
from services.managers import TokenManager, ModelManager
from dependencies.deps import get_token_manager, get_model_manager
from core.auth import require_roles
from core.logger import logger
from core.config import settings
from core.limiter import limiter

forecast_router = APIRouter(
    prefix="/forecast",
    responses={404: {"description": "Not found"}},
    dependencies=[Depends(require_roles(["Admin", "User"])), 
                  Depends(get_token_manager),
                  Depends(get_model_manager)]
)


@forecast_router.post("")
@limiter.limit("100/minute")
async def forecast(
    request: Request,
    data: ForecastRequest,
    token_manager: TokenManager = Depends(get_token_manager),
    model_manager: ModelManager = Depends(get_model_manager)
):
    
    url = f"{settings.powerservice_baseurl}/powerdata/inference/forecast" # Build API url
    headers = {"Authorization": f"Bearer {token_manager.get_token()}"} # Set JWT access token
    payload = {
        "country": data.country, 
        "forecastdate": data.forecast_date,
        "interval": data.interval,
        "horizon": data.horizon
    }

    try:
        response = requests.post(url, timeout=5, json=payload, headers=headers) # Send request to powerservice which returns input data for inference
        response.raise_for_status()

        country = parse_country(data.country) # Create Country object
        interval = parse_interval(data.interval) # Create Interval object
        horizon = parse_forecast_horizon(data.horizon) # Create ForecastHorizon object
        
        model = model_manager.get_forecast_model(country, interval, horizon) # Get model for current country, interval and horizon
        y_pred, hist, ramp = model_predict_from_response(response, model, interval, ModelType.FORECAST) # Predict using current model

        timestamps = create_forecast_timestamps(data.forecast_date, interval, horizon) # Create timestamps before and after forecast date

        return {"predicted": y_pred.tolist(), "labels": timestamps.tolist(), "hist": hist, "ramp": ramp.tolist()}

    except requests.exceptions.RequestException as e:
        logger.error(f"Error pinging API: {e}")