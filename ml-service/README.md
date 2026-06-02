# Inferra ML Service

FastAPI microservice for crypto price forecasting. It is designed for a diploma project and keeps the architecture practical: two public pipeline endpoints, three model adapters, local model persistence, and a small async client for the .NET API.

## Public Endpoints

- `POST /train` - fetches training datasets from .NET, trains all models for each token, and saves models locally.
- `POST /predict-daily` - fetches forecast datasets from .NET, loads saved models, generates 14-day forecasts, and posts the result back to .NET.

The service trains up to `MAX_SYMBOLS=10` tokens.

## Project Structure

```text
app/
  clients/      async .NET API client
  ml_models/    Linear Regression, XGBoost, LSTM adapters
  routers/      FastAPI routes
  schemas/      Pydantic DTOs
  services/     training, prediction, model storage, jobs
  utils/        dataframe features and metrics
main.py         FastAPI entrypoint
```

## Local Setup

```powershell
.\.venv\Scripts\python.exe -m pip install -r requirements.txt
.\.venv\Scripts\python.exe -m uvicorn main:app --reload
```

Swagger UI:

```text
http://127.0.0.1:8000/docs
```

By default `.env` has `USE_LOCAL_DATA=false`, so the service talks to the .NET API configured by `DOTNET_API_BASE_URL`. In local development this is `https://localhost:7071`, matching the backend HTTPS launch URL. `DOTNET_API_VERIFY_SSL=false` is used for the ASP.NET local development certificate. `REQUEST_TIMEOUT_SECONDS=600` leaves room for debugging breakpoints while the .NET API is processing a request. If the .NET API is not running yet, requests wait up to `DOTNET_API_WAIT_TIMEOUT_SECONDS` and retry every `DOTNET_API_WAIT_INTERVAL_SECONDS`.

Set `USE_LOCAL_DATA=true` only when you want to run the ML service from `test-data.json` without the .NET API.

## .NET API Contract

Training dataset:

```http
GET /api/ml/datasets/training
```

Forecast dataset:

```http
GET /api/ml/datasets/forecast
```

Daily forecast result:

```http
POST /api/ml/daily
```

## Models

- `LinearRegression` - scikit-learn model using OHLCV and simple rolling features.
- `XGBoost` - tree-based regression using the same tabular features.
- `LSTM` - Keras neural network using close-price sliding windows and `MinMaxScaler`.

Metrics logged after training:

- `MAE`
- `RMSE`
