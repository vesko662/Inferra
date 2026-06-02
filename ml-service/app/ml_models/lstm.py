from __future__ import annotations

from datetime import timedelta

import numpy as np
from sklearn.preprocessing import MinMaxScaler

from app.core import settings
from app.ml_models.base import CryptoModel, ForecastPoint
from app.services.model_store import ModelStore
from app.utils.dataframes import append_predicted_close
from app.utils.metrics import regression_metrics


class LSTMCryptoModel(CryptoModel):
    model_type = "LSTM"
    storage_key = "lstm"

    def __init__(self, model_store: ModelStore) -> None:
        self.model_store = model_store

    def train(self, symbol: str, frame):
        keras = self._load_keras()
        close_values = frame[["close"]].astype(float).to_numpy()
        window_size = settings.lstm_window_size
        if len(close_values) < window_size + 30:
            raise ValueError(f"At least {window_size + 30} candles are required for LSTM training.")

        scaler = MinMaxScaler(feature_range=(0, 1))
        scaled = scaler.fit_transform(close_values)
        x, y = self._create_windows(scaled, window_size)

        split_index = max(1, int(len(x) * 0.8))
        x_train, x_test = x[:split_index], x[split_index:]
        y_train, y_test = y[:split_index], y[split_index:]

        model = keras.Sequential(
            [
                keras.layers.Input(shape=(window_size, 1)),
                keras.layers.LSTM(32),
                keras.layers.Dense(1),
            ]
        )
        model.compile(optimizer="adam", loss="mse")
        model.fit(
            x_train,
            y_train,
            epochs=settings.lstm_epochs,
            batch_size=settings.lstm_batch_size,
            verbose=0,
        )

        predictions_scaled = model.predict(x_test, verbose=0) if len(x_test) else np.array([])
        predictions = scaler.inverse_transform(predictions_scaled).flatten() if len(predictions_scaled) else np.array([])
        y_actual = scaler.inverse_transform(y_test.reshape(-1, 1)).flatten() if len(y_test) else np.array([])
        metrics = regression_metrics(y_actual, predictions)

        model_path = self.model_store.keras_path_for(symbol, self.storage_key)
        model_path.parent.mkdir(parents=True, exist_ok=True)
        model.save(model_path)

        self.model_store.save(
            symbol,
            self.storage_key,
            {
                "model_type": self.model_type,
                "model_version": settings.model_version,
                "model_path": str(model_path),
                "scaler": scaler,
                "window_size": window_size,
                "metrics": metrics,
                "latest_training_date": str(frame.iloc[-1]["date"].date()),
            },
        )
        return metrics

    def forecast(self, symbol: str, frame, horizon_days: int) -> list[ForecastPoint]:
        keras = self._load_keras()
        bundle = self.model_store.load(symbol, self.storage_key)
        model = keras.models.load_model(bundle["model_path"])
        scaler = bundle["scaler"]
        window_size = bundle["window_size"]
        working = frame.copy()
        points: list[ForecastPoint] = []

        for day_offset in range(1, horizon_days + 1):
            close_values = working[["close"]].astype(float).to_numpy()
            scaled = scaler.transform(close_values)
            latest_window = scaled[-window_size:].reshape(1, window_size, 1)
            predicted_scaled = model.predict(latest_window, verbose=0)
            predicted_price = float(scaler.inverse_transform(predicted_scaled)[0][0])
            target_date = working.iloc[-1]["date"] + timedelta(days=1)
            working = append_predicted_close(working, target_date, predicted_price)
            points.append(ForecastPoint(day_offset, target_date.date(), predicted_price))

        return points

    def _create_windows(self, scaled_values: np.ndarray, window_size: int):
        x, y = [], []
        for index in range(window_size, len(scaled_values)):
            x.append(scaled_values[index - window_size:index])
            y.append(scaled_values[index, 0])
        return np.array(x), np.array(y)

    def _load_keras(self):
        try:
            from tensorflow import keras
        except ImportError as exc:
            raise RuntimeError(
                "TensorFlow is not installed. Install tensorflow to train or run LSTM forecasts."
            ) from exc
        return keras
