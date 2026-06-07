from __future__ import annotations

import logging

from fastapi import APIRouter, HTTPException

from app.schemas.jobs import PipelineRunResult
from app.schemas.sentiment import SentimentPredictRequest, SentimentPredictResponse
from app.services.model_store import ModelStore
from app.services.sentiment_service import SentimentPredictionService, SentimentTrainingService


logger = logging.getLogger(__name__)
router = APIRouter(prefix="/sentiment", tags=["Sentiment"])


@router.post("/train", response_model=PipelineRunResult)
async def train_sentiment() -> PipelineRunResult:
    service = SentimentTrainingService(ModelStore())
    return service.train()


@router.post("/predict", response_model=list[SentimentPredictResponse])
async def predict_sentiment(requests: list[SentimentPredictRequest]) -> list[SentimentPredictResponse]:
    if not requests:
        raise HTTPException(status_code=422, detail="requests list must not be empty.")

    try:
        service = SentimentPredictionService(ModelStore())
        results = []
        for request in requests:
            if not request.texts:
                raise HTTPException(status_code=422, detail=f"texts for symbol '{request.symbol}' must not be empty.")
            sentiments = service.predict(request.texts)
            results.append(SentimentPredictResponse(
                symbol=request.symbol,
                bullish=sentiments.count("bullish"),
                neutral=sentiments.count("neutral"),
                bearish=sentiments.count("bearish"),
            ))
    except FileNotFoundError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc

    return results