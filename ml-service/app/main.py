from __future__ import annotations

from fastapi import FastAPI

from app.core import configure_logging, settings
from app.routers.ml import router as ml_router


configure_logging()

app = FastAPI(title=settings.app_name)
app.include_router(ml_router)


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}

