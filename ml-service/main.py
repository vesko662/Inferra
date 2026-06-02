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


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "main:app",
        host="127.0.0.1",
        port=8000,
        reload=False,
    )
