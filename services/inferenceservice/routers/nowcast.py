from fastapi import APIRouter, Depends, Request
import requests
from utils.helpers import model_predict_from_response, calculate_historic_predict_count
from schemas.requests import LatestRequest, HistoryRequest
from core.enums import ModelType, parse_interval, parse_country, get_interval_multiplier
from services.managers import TokenManager, ModelManager
from dependencies.deps import get_token_manager, get_model_manager
from core.auth import require_roles
from core.logger import logger
from core.config import settings
from core.limiter import limiter

nowcast_router = APIRouter(
    prefix="/nowcast",
    responses={404: {"description": "Not found"}},
    dependencies=[Depends(require_roles(["Admin", "User"])), 
                  Depends(get_token_manager),
                  Depends(get_model_manager)]
)

@nowcast_router.post("/latest")
@limiter.limit("100/minute")
async def predict(
    request: Request,
    data: LatestRequest,
    token_manager: TokenManager = Depends(get_token_manager),
    model_manager: ModelManager = Depends(get_model_manager)
):
    
    url = f"{settings.powerservice_baseurl}/powerdata/inference/latest" # Build API url
    headers = {"Authorization": f"Bearer {token_manager.get_token()}"} # Set JWT access token
    payload = {
        "country": data.country, 
        "count": data.count, 
        "interval": data.interval
    }

    try:
        response = requests.post(url, timeout=5, headers=headers, json=payload) # Send request to powerservice which returns input data for inference
        response.raise_for_status()

        country = parse_country(data.country) # Create Country object
        interval = parse_interval(data.interval) # Create Interval object
        multiplier = get_interval_multiplier(interval) # Create multiplier factor based on interval type
        predict_count = data.count * multiplier.value # Calculate number of timestamps to predict

        model = model_manager.get_nowcast_model(country, interval) # Get model for current country and interval 
        y_pred, hist, ramp = model_predict_from_response(response, model, interval, ModelType.NOWCAST, predict_count) # Predict using current model
        return {"predicted": y_pred.tolist(), "hist": hist, "ramp": ramp.tolist()}

    except requests.exceptions.RequestException as e:
        logger.error(f"Error pinging API: {e}")

@nowcast_router.post("/history")
@limiter.limit("100/minute")
async def predict(
    request: Request,
    data: HistoryRequest,
    token_manager: TokenManager = Depends(get_token_manager),
    model_manager: ModelManager = Depends(get_model_manager)
):
    
    url = f"{settings.powerservice_baseurl}/powerdata/inference/history"
    headers = {"Authorization": f"Bearer {token_manager.get_token()}"}
    payload = {
        "country": data.country, 
        "startdate": data.start_date, 
        "enddate": data.end_date, 
        "interval": data.interval
    }

    try:
        response = requests.post(url, timeout=5, headers=headers, json=payload)
        response.raise_for_status()

        country = parse_country(data.country) # Create Country object
        interval = parse_interval(data.interval) # Create Interval object
        start_date = data.start_date
        end_date = data.end_date
        predict_count = calculate_historic_predict_count(start_date, end_date, interval)
        
        model = model_manager.get_nowcast_model(country, interval)
        y_pred, hist, ramp = model_predict_from_response(response, model, interval, ModelType.NOWCAST, predict_count)
        return {"predicted": y_pred.tolist(), "hist": hist, "ramp": ramp.tolist()}

    except requests.exceptions.RequestException as e:
        logger.error(f"Error pinging API: {e}")