from __future__ import annotations

import math

import pandas as pd

from app.schemas.datasets import CandleDto


REGRESSION_FEATURES = [
    "open",
    "high",
    "low",
    "close",
    "volume",
    "close_change_1d",
    "close_change_7d",
    "volume_change_1d",
    "ma_7",
    "ma_14",
]


def candles_to_dataframe(candles: list[CandleDto]) -> pd.DataFrame:
    frame = pd.DataFrame([candle.model_dump(mode="json") for candle in candles])
    required_columns = {"date", "open", "high", "low", "close", "volume"}
    missing_columns = required_columns - set(frame.columns)
    if missing_columns:
        raise ValueError(f"Missing candle columns: {', '.join(sorted(missing_columns))}")

    frame["date"] = pd.to_datetime(frame["date"], utc=True)
    frame = frame.sort_values("date").reset_index(drop=True)

    numeric_columns = ["open", "high", "low", "close", "volume"]
    frame[numeric_columns] = frame[numeric_columns].apply(pd.to_numeric, errors="coerce")
    return frame.dropna(subset=numeric_columns)


def add_regression_features(frame: pd.DataFrame) -> pd.DataFrame:
    featured = frame.copy()
    featured["close_change_1d"] = featured["close"].pct_change()
    featured["close_change_7d"] = featured["close"].pct_change(periods=7)
    featured["volume_change_1d"] = featured["volume"].pct_change()
    featured["ma_7"] = featured["close"].rolling(window=7).mean()
    featured["ma_14"] = featured["close"].rolling(window=14).mean()
    featured = featured.replace([math.inf, -math.inf], pd.NA)
    return featured.dropna(subset=REGRESSION_FEATURES).reset_index(drop=True)


def append_predicted_close(frame: pd.DataFrame, target_date: pd.Timestamp, predicted_close: float) -> pd.DataFrame:
    previous = frame.iloc[-1]
    next_row = {
        "date": target_date,
        "open": float(previous["close"]),
        "high": max(float(previous["close"]), predicted_close),
        "low": min(float(previous["close"]), predicted_close),
        "close": predicted_close,
        "volume": float(previous["volume"]),
    }
    return pd.concat([frame, pd.DataFrame([next_row])], ignore_index=True)

