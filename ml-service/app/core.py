from __future__ import annotations

import logging
import os
from pathlib import Path


BASE_DIR = Path(__file__).resolve().parent.parent


def load_dotenv(path: Path = BASE_DIR / ".env") -> None:
    if not path.exists():
        return

    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue
        key, value = line.split("=", 1)
        os.environ.setdefault(key.strip(), value.strip().strip('"').strip("'"))


load_dotenv()


class Settings:
    app_name: str = os.getenv("APP_NAME", "Inferra ML Service")
    dotnet_api_base_url: str = os.getenv("DOTNET_API_BASE_URL", "https://localhost:7071")
    request_timeout_seconds: float = float(os.getenv("REQUEST_TIMEOUT_SECONDS", "600"))
    dotnet_api_wait_timeout_seconds: float = float(os.getenv("DOTNET_API_WAIT_TIMEOUT_SECONDS", "300"))
    dotnet_api_wait_interval_seconds: float = float(os.getenv("DOTNET_API_WAIT_INTERVAL_SECONDS", "2"))
    dotnet_api_verify_ssl: bool = os.getenv("DOTNET_API_VERIFY_SSL", "false").lower() == "true"
    use_local_data: bool = os.getenv("USE_LOCAL_DATA", "true").lower() == "true"
    local_data_path: Path = BASE_DIR / os.getenv("LOCAL_DATA_PATH", "test-data.json")
    models_dir: Path = BASE_DIR / os.getenv("MODELS_DIR", "models")
    forecast_horizon_days: int = int(os.getenv("FORECAST_HORIZON_DAYS", "14"))
    max_symbols: int = int(os.getenv("MAX_SYMBOLS", "10"))
    lstm_window_size: int = int(os.getenv("LSTM_WINDOW_SIZE", "30"))
    lstm_epochs: int = int(os.getenv("LSTM_EPOCHS", "12"))
    lstm_batch_size: int = int(os.getenv("LSTM_BATCH_SIZE", "16"))
    model_version: str = os.getenv("MODEL_VERSION", "v1")
    log_level: str = os.getenv("LOG_LEVEL", "INFO")
    run_pipelines_in_background: bool = os.getenv("RUN_PIPELINES_IN_BACKGROUND", "true").lower() == "true"
    enabled_models: list[str] = [
        model.strip()
        for model in os.getenv("ENABLED_MODELS", "LinearRegression,XGBoost,LSTM").split(",")
        if model.strip()
    ]


settings = Settings()


def configure_logging() -> None:
    logging.basicConfig(
        level=settings.log_level.upper(),
        format="%(asctime)s %(levelname)s [%(name)s] %(message)s",
    )
