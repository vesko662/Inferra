from __future__ import annotations

from datetime import timedelta

from sklearn.linear_model import LinearRegression

from app.core import settings
from app.ml_models.base import CryptoModel, ForecastPoint
from app.services.model_store import ModelStore
from app.utils.dataframes import REGRESSION_FEATURES, add_regression_features, append_predicted_close
from app.utils.metrics import regression_metrics


class LinearRegressionCryptoModel(CryptoModel):
    model_type = "LinearRegression"
    storage_key = "linear-regression"

    def __init__(self, model_store: ModelStore) -> None:
        self.model_store = model_store

    def train(self, symbol: str, frame):
        featured = add_regression_features(frame)
        if len(featured) < 60:
            raise ValueError("At least 60 candles are required for Linear Regression training.")

        dataset = featured.copy()
        dataset["target_close"] = dataset["close"].shift(-1)
        dataset = dataset.dropna(subset=["target_close"])

        split_index = max(1, int(len(dataset) * 0.8))
        x = dataset[REGRESSION_FEATURES]
        y = dataset["target_close"]

        x_train, x_test = x.iloc[:split_index], x.iloc[split_index:]
        y_train, y_test = y.iloc[:split_index], y.iloc[split_index:]

        model = LinearRegression()
        model.fit(x_train, y_train)
        predictions = model.predict(x_test) if not x_test.empty else []
        metrics = regression_metrics(y_test.to_numpy(), predictions)

        self.model_store.save(
            symbol,
            self.storage_key,
            {
                "model_type": self.model_type,
                "model_version": settings.model_version,
                "model": model,
                "features": REGRESSION_FEATURES,
                "metrics": metrics,
                "latest_training_date": str(dataset.iloc[-1]["date"].date()),
            },
        )
        return metrics

    def forecast(self, symbol: str, frame, horizon_days: int) -> list[ForecastPoint]:
        bundle = self.model_store.load(symbol, self.storage_key)
        working = frame.copy()
        points: list[ForecastPoint] = []

        for day_offset in range(1, horizon_days + 1):
            featured = add_regression_features(working)
            latest = featured.iloc[[-1]]
            predicted_price = float(bundle["model"].predict(latest[bundle["features"]])[0])
            target_date = latest.iloc[0]["date"] + timedelta(days=1)
            working = append_predicted_close(working, target_date, predicted_price)
            points.append(ForecastPoint(day_offset, target_date.date(), predicted_price))

        return points

