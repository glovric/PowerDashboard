from fastapi import Request
from services.managers import TokenManager, ModelManager

async def get_token_manager(request: Request) -> TokenManager:
    return request.app.state.token_manager

async def get_model_manager(request: Request) -> ModelManager:
    return request.app.state.model_manager