from __future__ import annotations

from datetime import date, datetime

from app.schemas.datasets import ApiModel


class ForecastPointDto(ApiModel):
    day_offset: int
    target_date: date
    predicted_price: float


class ModelForecastDto(ApiModel):
    model_type: str
    model_version: str
    forecast_start_date: date
    forecast_end_date: date
    horizon_days: int
    points: list[ForecastPointDto]


class SymbolForecastDto(ApiModel):
    symbol: str
    generated_at: datetime
    forecasts: list[ModelForecastDto]

