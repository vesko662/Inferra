from __future__ import annotations

from fastapi import Header, HTTPException

from app.core import settings


async def verify_api_key(x_api_key: str = Header(...)) -> None:
    if not settings.ml_service_api_key or x_api_key != settings.ml_service_api_key:
        raise HTTPException(status_code=401, detail="Unauthorized.")