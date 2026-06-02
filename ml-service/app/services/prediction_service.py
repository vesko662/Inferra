from __future__ import annotations

import logging
from datetime import datetime, timezone

from app.clients.dotnet_api_client import DotNetApiClient
from app.core import settings
from app.ml_models.registry import get_models
from app.schemas.forecasts import ForecastPointDto, ModelForecastDto, SymbolForecastDto
from app.schemas.jobs import PipelineRunResult
from app.services.model_store import ModelStore
from app.utils.dataframes import candles_to_dataframe


logger = logging.getLogger(__name__)


class PredictionService:
    def __init__(self, api_client: DotNetApiClient, model_store: ModelStore) -> None:
        self.api_client = api_client
        self.models = get_models(model_store)

    async def predict_daily(self) -> PipelineRunResult:
        datasets = await self.api_client.get_forecast_datasets()
        datasets = datasets[: settings.max_symbols]
        generated_at = datetime.now(timezone.utc)
        payloads: list[SymbolForecastDto] = []
        processed_symbols: list[str] = []
        errors: dict[str, str] = {}

        logger.info("Starting daily prediction pipeline for %s symbols.", len(datasets))

        for dataset in datasets:
            symbol = dataset.symbol.upper()
            try:
                frame = candles_to_dataframe(dataset.candles)
            except Exception as exc:  # noqa: BLE001
                errors[symbol] = f"Dataset error: {exc}"
                logger.exception("Invalid forecast dataset for %s.", symbol)
                continue

            model_forecasts: list[ModelForecastDto] = []
            for model in self.models:
                key = f"{symbol}:{model.model_type}"
                try:
                    logger.info("Forecasting %s for %s started.", model.model_type, symbol)
                    points = model.forecast(symbol, frame, settings.forecast_horizon_days)
                    forecast_points = [
                        ForecastPointDto(
                            day_offset=point.day_offset,
                            target_date=point.target_date,
                            predicted_price=point.predicted_price,
                        )
                        for point in points
                    ]
                    model_forecasts.append(
                        ModelForecastDto(
                            model_type=model.model_type,
                            model_version=settings.model_version,
                            forecast_start_date=forecast_points[0].target_date,
                            forecast_end_date=forecast_points[-1].target_date,
                            horizon_days=settings.forecast_horizon_days,
                            points=forecast_points,
                        )
                    )
                    processed_symbols.append(key)
                    logger.info("Generated %s-day %s forecast for %s.", settings.forecast_horizon_days, model.model_type, symbol)
                    formatted_prices = ", ".join(f"{point.predicted_price:.6f}" for point in forecast_points)
                    logger.info("%s %s predicted prices: %s", symbol, model.model_type, formatted_prices)
                except Exception as exc:  # noqa: BLE001
                    errors[key] = str(exc)
                    logger.exception("Forecast failed for %s.", key)

            if model_forecasts:
                payloads.append(
                    SymbolForecastDto(
                        symbol=symbol,
                        generated_at=generated_at,
                        forecasts=model_forecasts,
                    )
                )

        if payloads:
            await self.api_client.post_daily_forecasts(payloads)

        status = "completed" if not errors else "failed"
        logger.info("Daily prediction pipeline finished with status=%s.", status)
        return PipelineRunResult(status=status, processed_symbols=processed_symbols, errors=errors)
