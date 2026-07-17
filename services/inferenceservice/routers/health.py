from fastapi import APIRouter, Request, Response
from core.limiter import limiter

health_router = APIRouter(
    prefix="/health",
    responses={404: {"description": "Not found"}}
)

@health_router.get("")
@limiter.limit("100/minute")
async def health(request: Request):
    return Response(status_code=204)