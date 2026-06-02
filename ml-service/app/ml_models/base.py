from __future__ import annotations

from abc import ABC, abstractmethod
from datetime import date

import pandas as pd


class ForecastPoint:
    def __init__(self, day_offset: int, target_date: date, predicted_price: float) -> None:
        self.day_offset = day_offset
        self.target_date = target_date
        self.predicted_price = predicted_price


class CryptoModel(ABC):
    model_type: str

    @abstractmethod
    def train(self, symbol: str, frame: pd.DataFrame) -> dict:
        raise NotImplementedError

    @abstractmethod
    def forecast(self, symbol: str, frame: pd.DataFrame, horizon_days: int) -> list[ForecastPoint]:
        raise NotImplementedError

