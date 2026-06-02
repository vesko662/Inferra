# Architecture

The service exposes only two ML pipeline endpoints:

- `POST /train`
- `POST /predict-daily`

The rest of the code is internal orchestration.

## Layers

```text
routers
  Receives HTTP requests and starts background jobs.

services
  Coordinates training, prediction, model persistence, and job state.

clients
  Handles async HTTP communication with the .NET API.

ml_models
  Contains model-specific implementations behind one common interface.

schemas
  Contains Pydantic DTOs for .NET API contracts and internal responses.

utils
  Contains dataframe feature generation and metric helpers.
```

## Training Flow

```text
POST /train
  -> DotNetApiClient.get_training_datasets()
  -> TrainingService.train_all()
  -> LinearRegression.train()
  -> XGBoost.train()
  -> LSTM.train()
  -> ModelStore.save()
```

## Prediction Flow

```text
POST /predict-daily
  -> DotNetApiClient.get_forecast_datasets()
  -> PredictionService.predict_daily()
  -> ModelStore.load()
  -> model.forecast(horizon_days=14)
  -> DotNetApiClient.post_daily_forecasts()
```

## Model Persistence

Tabular models are saved as `.joblib` files:

```text
models/linear-regression/BTC.joblib
models/xgboost/BTC.joblib
```

LSTM stores the Keras model separately and metadata/scaler in joblib:

```text
models/lstm/BTC.keras
models/lstm/BTC.joblib
```

