from __future__ import annotations

from pathlib import Path
from typing import Any

import joblib

from app.core import settings


class ModelStore:
    def __init__(self, base_dir: Path | None = None) -> None:
        self.base_dir = base_dir or settings.models_dir

    def save(self, symbol: str, model_type: str, payload: dict[str, Any]) -> Path:
        path = self.path_for(symbol, model_type)
        path.parent.mkdir(parents=True, exist_ok=True)
        joblib.dump(payload, path)
        return path

    def load(self, symbol: str, model_type: str) -> dict[str, Any]:
        path = self.path_for(symbol, model_type)
        if not path.exists():
            raise FileNotFoundError(f"Model not found for {symbol.upper()} / {model_type}: {path}")
        return joblib.load(path)

    def path_for(self, symbol: str, model_type: str) -> Path:
        return self.base_dir / model_type / f"{symbol.upper()}.joblib"

    def keras_path_for(self, symbol: str, model_type: str) -> Path:
        return self.base_dir / model_type / f"{symbol.upper()}.keras"
