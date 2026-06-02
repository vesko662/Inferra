from __future__ import annotations

import logging

from app.clients.dotnet_api_client import DotNetApiClient
from app.core import settings
from app.ml_models.registry import get_models
from app.schemas.jobs import PipelineRunResult
from app.services.model_store import ModelStore
from app.utils.dataframes import candles_to_dataframe


logger = logging.getLogger(__name__)


class TrainingService:
    def __init__(self, api_client: DotNetApiClient, model_store: ModelStore) -> None:
        self.api_client = api_client
        self.models = get_models(model_store)

    async def train_all(self) -> PipelineRunResult:
        datasets = await self.api_client.get_training_datasets()
        datasets = datasets[: settings.max_symbols]
        processed_symbols: list[str] = []
        errors: dict[str, str] = {}

        logger.info("Starting training pipeline for %s symbols.", len(datasets))

        for dataset in datasets:
            symbol = dataset.symbol.upper()
            try:
                frame = candles_to_dataframe(dataset.candles)
            except Exception as exc:  # noqa: BLE001
                errors[symbol] = f"Dataset error: {exc}"
                logger.exception("Invalid training dataset for %s.", symbol)
                continue

            for model in self.models:
                key = f"{symbol}:{model.model_type}"
                try:
                    logger.info("Training %s for %s started.", model.model_type, symbol)
                    metrics = model.train(symbol, frame)
                    processed_symbols.append(key)
                    logger.info("Trained %s for %s. Metrics: %s", model.model_type, symbol, metrics)
                except Exception as exc:  # noqa: BLE001
                    errors[key] = str(exc)
                    logger.exception("Training failed for %s.", key)

        status = "completed" if not errors else "failed"
        logger.info("Training pipeline finished with status=%s.", status)
        return PipelineRunResult(status=status, processed_symbols=processed_symbols, errors=errors)
