from pydantic import BaseModel

class LatestRequest(BaseModel):
    country: str
    count: int
    interval: int

class HistoryRequest(BaseModel):
    country: str
    start_date: str
    end_date: str
    interval: int

class ForecastRequest(BaseModel):
    country: str
    forecast_date: str
    interval: int
    horizon: int