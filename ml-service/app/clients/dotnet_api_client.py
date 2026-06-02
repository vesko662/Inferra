from __future__ import annotations

import asyncio
import json
import logging
from pathlib import Path
from typing import Any

import httpx

from app.core import settings
from app.schemas.datasets import AssetDatasetDto
from app.schemas.forecasts import SymbolForecastDto


logger = logging.getLogger(__name__)


class DotNetApiClient:
    def __init__(self) -> None:
        self.base_url = settings.dotnet_api_base_url.rstrip("/")
        self.timeout = settings.request_timeout_seconds
        self.wait_timeout = settings.dotnet_api_wait_timeout_seconds
        self.wait_interval = settings.dotnet_api_wait_interval_seconds
        self.verify_ssl = settings.dotnet_api_verify_ssl

    async def get_training_datasets(self) -> list[AssetDatasetDto]:
        if settings.use_local_data:
            logger.info("Loading training datasets from local file: %s", settings.local_data_path)
            return self._load_local_datasets(settings.local_data_path)
        return await self._get_datasets("/api/ml/datasets/training")

    async def get_forecast_datasets(self) -> list[AssetDatasetDto]:
        if settings.use_local_data:
            logger.info("Loading forecast datasets from local file: %s", settings.local_data_path)
            return self._load_local_datasets(settings.local_data_path)
        return await self._get_datasets("/api/ml/datasets/forecast")

    async def post_daily_forecasts(self, forecasts: list[SymbolForecastDto]) -> None:
        if settings.use_local_data:
            logger.info("Local data mode enabled; skipping POST of %s forecast payloads.", len(forecasts))
            return

        for forecast in forecasts:
            payload = forecast.model_dump(by_alias=True, mode="json")
            logger.info(
                "Posting daily forecast for %s with %s model forecasts to .NET API.",
                forecast.symbol,
                len(forecast.forecasts),
            )
            response = await self._request_with_api_wait("POST", "/api/ml/daily", json=payload)
            response.raise_for_status()
            logger.info("Posted daily forecast for %s to .NET API.", forecast.symbol)
        logger.info("Posted %s daily forecast payloads to .NET API.", len(forecasts))

    async def _get_datasets(self, endpoint: str) -> list[AssetDatasetDto]:
        response = await self._request_with_api_wait("GET", endpoint)
        response.raise_for_status()
        raw_items = self._extract_dataset_assets(response.json())
        return [AssetDatasetDto.model_validate(item) for item in raw_items]

    def _extract_dataset_assets(self, raw: Any) -> list[Any]:
        if isinstance(raw, dict):
            raw_assets = raw.get("assets")
            if raw_assets is None:
                raise ValueError("Dataset response from .NET API does not contain an 'assets' field.")
            if not isinstance(raw_assets, list):
                raise ValueError("Dataset response 'assets' field must be a list.")
            return raw_assets

        if isinstance(raw, list):
            return raw

        raise ValueError("Dataset response from .NET API must be a list or an object with an 'assets' field.")

    async def _request_with_api_wait(self, method: str, endpoint: str, **kwargs: Any) -> httpx.Response:
        deadline = None if self.wait_timeout <= 0 else asyncio.get_running_loop().time() + self.wait_timeout
        attempt = 1

        async with httpx.AsyncClient(
            base_url=self.base_url,
            timeout=self.timeout,
            verify=self.verify_ssl,
            follow_redirects=True,
        ) as client:
            while True:
                try:
                    return await client.request(method, endpoint, **kwargs)
                except httpx.ConnectError as exc:
                    now = asyncio.get_running_loop().time()
                    if deadline is not None and now >= deadline:
                        raise RuntimeError(
                            f"Could not connect to .NET API at {self.base_url}{endpoint} "
                            f"after {self.wait_timeout:g} seconds. Start the .NET API, "
                            "update DOTNET_API_BASE_URL, or set USE_LOCAL_DATA=true."
                        ) from exc

                    logger.info(
                        "Waiting for .NET API at %s%s before %s request; attempt %s.",
                        self.base_url,
                        endpoint,
                        method,
                        attempt,
                    )
                    attempt += 1
                    await asyncio.sleep(self.wait_interval)

    def _load_local_datasets(self, path: Path) -> list[AssetDatasetDto]:
        if not path.exists():
            raise FileNotFoundError(f"Local dataset file not found: {path}")

        raw = json.loads(path.read_text(encoding="utf-8"))
        raw_items = raw.get("assets", raw) if isinstance(raw, dict) else raw
        return [AssetDatasetDto.model_validate(item) for item in raw_items]
