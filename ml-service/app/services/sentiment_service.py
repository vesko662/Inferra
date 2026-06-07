from __future__ import annotations

import logging

import pandas as pd

from app.core import settings
from app.ml_models.sentiment_svm import SentimentSVMModel
from app.schemas.jobs import PipelineRunResult
from app.services.model_store import ModelStore


logger = logging.getLogger(__name__)


class SentimentTrainingService:
    def __init__(self, model_store: ModelStore) -> None:
        self.model = SentimentSVMModel(model_store)

    def train(self) -> PipelineRunResult:
        path = settings.sentiment_data_path
        if not path.exists():
            raise FileNotFoundError(f"Sentiment dataset not found: {path}")

        logger.info("Loading sentiment dataset from %s", path)
        df = pd.read_parquet(path)

        texts = df["text"].astype(str).tolist()
        labels = df["market_direction"].astype(int).tolist()
        logger.info("Training SentimentSVM on %s samples.", len(texts))

        metrics = self.model.train(texts, labels)

        logger.info(
            "SentimentSVM training finished. Accuracy=%.4f",
            metrics.get("accuracy", 0.0),
        )
        logger.info("Classification report:\n%s", metrics.get("report", ""))

        return PipelineRunResult(
            status="completed",
            processed_symbols=["SentimentSVM"],
            errors={},
        )


class SentimentPredictionService:
    def __init__(self, model_store: ModelStore) -> None:
        self.model = SentimentSVMModel(model_store)

    def predict(self, texts: list[str]) -> list[str]:
        return self.model.predict(texts)
