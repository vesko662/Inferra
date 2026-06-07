from __future__ import annotations

from pydantic import BaseModel

from app.schemas.datasets import ApiModel


class SentimentPredictRequest(BaseModel):
    symbol: str
    texts: list[str]


class SentimentPredictResponse(ApiModel):
    symbol: str
    bullish: int
    neutral: int
    bearish: int