from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from slowapi import _rate_limit_exceeded_handler
from slowapi.errors import RateLimitExceeded
from utils.helpers import empty_response_handler
from services.managers import TokenManager, ModelManager
from routers.nowcast import nowcast_router
from routers.forecast import forecast_router
from routers.health import health_router
from core.limiter import limiter

app = FastAPI(
    title="inferenceservice",
    description="FastAPI service for model inference",
    version="1.0.0"
)

@app.on_event("startup")
async def startup_event():
    app.state.token_manager = TokenManager()
    app.state.model_manager = ModelManager()
    app.state.limiter = limiter

origins = [
    "http://localhost:8080",
]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.add_exception_handler(HTTPException, empty_response_handler)
app.add_exception_handler(RateLimitExceeded, _rate_limit_exceeded_handler)

app.include_router(nowcast_router)
app.include_router(forecast_router)
app.include_router(health_router)