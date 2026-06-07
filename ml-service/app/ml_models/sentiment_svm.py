from __future__ import annotations

import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics import accuracy_score, classification_report
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline
from sklearn.svm import LinearSVC

from app.core import settings
from app.services.model_store import ModelStore


LABEL_MAP = {0: "neutral", 1: "bearish", 2: "bullish"}
INVERSE_LABEL_MAP = {v: k for k, v in LABEL_MAP.items()}


class SentimentSVMModel:
    model_type = "SentimentSVM"
    storage_key = "sentiment-svm"

    def __init__(self, model_store: ModelStore) -> None:
        self.model_store = model_store

    def train(self, texts: list[str], labels: list[int]) -> dict:
        x_train, x_test, y_train, y_test = train_test_split(
            texts, labels, test_size=0.2, random_state=42, stratify=labels
        )

        pipeline = Pipeline([
            ("tfidf", TfidfVectorizer(max_features=50_000, ngram_range=(1, 2), sublinear_tf=True)),
            ("clf", LinearSVC(C=1.0, max_iter=2000)),
        ])
        pipeline.fit(x_train, y_train)

        predictions = pipeline.predict(x_test)
        metrics = {
            "accuracy": float(accuracy_score(y_test, predictions)),
            "report": classification_report(y_test, predictions, target_names=["neutral", "bearish", "bullish"]),
        }

        self.model_store.save(
            "global",
            self.storage_key,
            {
                "model_type": self.model_type,
                "model_version": settings.model_version,
                "pipeline": pipeline,
                "label_map": LABEL_MAP,
            },
        )
        return metrics

    def predict(self, texts: list[str]) -> list[str]:
        bundle = self.model_store.load("global", self.storage_key)
        predictions = bundle["pipeline"].predict(texts)
        label_map: dict[int, str] = bundle["label_map"]
        return [label_map[int(p)] for p in predictions]
