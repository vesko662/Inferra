from __future__ import annotations

from app.ml_models.base import CryptoModel
from app.ml_models.linear_regression import LinearRegressionCryptoModel
from app.ml_models.lstm import LSTMCryptoModel
from app.ml_models.xgboost_model import XGBoostCryptoModel
from app.services.model_store import ModelStore
from app.core import settings


def get_models(model_store: ModelStore) -> list[CryptoModel]:
    models: list[CryptoModel] = [
        LinearRegressionCryptoModel(model_store),
        XGBoostCryptoModel(model_store),
        LSTMCryptoModel(model_store),
    ]

    enabled_model_names = {model_name.lower() for model_name in settings.enabled_models}
    return [model for model in models if model.model_type.lower() in enabled_model_names]
