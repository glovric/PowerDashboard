from typing import Union, List
import jwt
from jwt import ExpiredSignatureError, InvalidTokenError
from fastapi import Request, HTTPException, status, Depends
from .config import settings
from .logger import logger

def verify_jwt_from_cookie(request: Request):
    token = request.cookies.get("jwt")

    if not token:
        logger.error(f"JWT token not found in cookies. Unauthorized")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Not authenticated"
        )

    try:
        payload = jwt.decode(
            token,
            settings.jwt_key,
            algorithms=["HS256"],
            audience=settings.jwt_audience,
            issuer=settings.jwt_issuer,
            options={
                "require": ["sub", "exp", "iss", "aud"]
            }
        )

        return payload

    except ExpiredSignatureError:
        logger.error(f"JWT token expired. Unauthorized")
        raise HTTPException(
            status_code=401,
            detail="Token expired"
        )

    except InvalidTokenError as e:
        logger.error(f"Invalid JWT token: {str(e)}")
        raise HTTPException(
            status_code=401,
            detail=f"Invalid token: {str(e)}"
        )

def require_roles(required_roles: Union[str, List[str]]):
    """
    Dependency factory that checks JWT roles.
    
    :param required_roles: single role string or list of role strings
    """
    if isinstance(required_roles, str):
        required_roles = [required_roles]

    def role_checker(user=Depends(verify_jwt_from_cookie)):
        token_roles = user.get("role")

        if token_roles is None:
            logger.error(f"Roles field not included in token. Forbidden")
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
            )

        # normalize token roles into a list
        if isinstance(token_roles, str):
            token_roles = [token_roles]

        # check if any of the required roles exist in token roles
        if not any(role in token_roles for role in required_roles):
            logger.error(f"Token does not include any of the allowed roles. Forbidden")
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN
            )

        return user

    return role_checker