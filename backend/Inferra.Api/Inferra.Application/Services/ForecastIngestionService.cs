using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Forecasts;
using Inferra.Domain.Entities;
using Inferra.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Services
{
    public class ForecastIngestionService : IForecastIngestionService
    {
        private static readonly ForecastModelType[] RequiredModelTypes =
        {
            ForecastModelType.LinearRegression,
            ForecastModelType.Lstm,
            ForecastModelType.XGBoost
        };

        private readonly IAssetRepository _assetRepository;
        private readonly IForecastRunRepository _forecastRunRepository;

        public ForecastIngestionService(
            IAssetRepository assetRepository,
            IForecastRunRepository forecastRunRepository)
        {
            _assetRepository = assetRepository;
            _forecastRunRepository = forecastRunRepository;
        }

        public async Task<SaveDailyForecastResult> SaveDailyForecastAsync(SaveDailyForecastRequest request)
        {
            var normalizedSymbol = NormalizeSymbol(request.Symbol);
            var asset = await _assetRepository.GetBySymbolAsync(normalizedSymbol);

            if (asset is null)
            {
                return SaveDailyForecastResult.NotFound();
            }

            var validationError = ValidateRequest(request);
            if (validationError is not null)
            {
                return SaveDailyForecastResult.Invalid(validationError);
            }

            var parsedForecasts = new List<(ForecastModelType ModelType, ModelForecastInputDto Forecast)>();

            foreach (var forecast in request.Forecasts)
            {
                if (!Enum.TryParse<ForecastModelType>(forecast.ModelType.Trim(), true, out var parsedModelType))
                {
                    return SaveDailyForecastResult.Invalid($"Unsupported model type '{forecast.ModelType}'.");
                }

                parsedForecasts.Add((parsedModelType, forecast));
            }

            var latestRuns = await _forecastRunRepository.GetLatestTrackedByAssetIdAndModelTypesAsync(
                asset.Id,
                parsedForecasts.Select(x => x.ModelType));

            foreach (var latestRun in latestRuns)
            {
                latestRun.IsLatest = false;
            }

            var generatedAt = request.GeneratedAt == default
                ? DateTime.UtcNow
                : request.GeneratedAt;

            var newRuns = parsedForecasts
                .OrderBy(x => x.ModelType)
                .Select(x => MapRun(asset.Id, generatedAt, x.ModelType, x.Forecast))
                .ToList();

            await _forecastRunRepository.AddRangeAsync(newRuns);
            await _forecastRunRepository.SaveChangesAsync();

            return SaveDailyForecastResult.Ok();
        }

        private static ForecastRun MapRun(
            int assetId,
            DateTime generatedAt,
            ForecastModelType modelType,
            ModelForecastInputDto forecast)
        {
            return new ForecastRun
            {
                AssetId = assetId,
                ModelType = modelType,
                ModelVersion = forecast.ModelVersion.Trim(),
                GeneratedAt = generatedAt,
                ForecastStartDate = forecast.ForecastStartDate,
                ForecastEndDate = forecast.ForecastEndDate,
                HorizonDays = forecast.HorizonDays,
                IsLatest = true,
                Points = forecast.Points
                    .OrderBy(x => x.DayOffset)
                    .Select(x => new ForecastPoint
                    {
                        DayOffset = x.DayOffset,
                        TargetDate = x.TargetDate,
                        PredictedPrice = x.PredictedPrice,
                        LowerBound = x.LowerBound,
                        UpperBound = x.UpperBound,
                        Confidence = x.Confidence
                    })
                    .ToList()
            };
        }
        private static string? ValidateRequest(SaveDailyForecastRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Symbol))
            {
                return "Symbol is required.";
            }

            if (request.Forecasts is null || request.Forecasts.Count != RequiredModelTypes.Length)
            {
                return "Exactly 3 forecasts are required: LinearRegression, Lstm, XGBoost.";
            }

            var parsedModelTypes = new HashSet<ForecastModelType>();

            foreach (var forecast in request.Forecasts)
            {
                if (string.IsNullOrWhiteSpace(forecast.ModelType))
                {
                    return "ModelType is required.";
                }

                if (!Enum.TryParse<ForecastModelType>(forecast.ModelType.Trim(), true, out var parsedModelType))
                {
                    return $"Unsupported model type '{forecast.ModelType}'.";
                }

                if (!parsedModelTypes.Add(parsedModelType))
                {
                    return $"Duplicate model type '{forecast.ModelType}'.";
                }

                if (string.IsNullOrWhiteSpace(forecast.ModelVersion))
                {
                    return $"ModelVersion is required for '{forecast.ModelType}'.";
                }

                if (forecast.HorizonDays <= 0)
                {
                    return $"HorizonDays must be greater than 0 for '{forecast.ModelType}'.";
                }

                if (forecast.ForecastStartDate > forecast.ForecastEndDate)
                {
                    return $"ForecastStartDate must be less than or equal to ForecastEndDate for '{forecast.ModelType}'.";
                }

                if (forecast.Points is null || forecast.Points.Count != forecast.HorizonDays)
                {
                    return $"Points count must match HorizonDays for '{forecast.ModelType}'.";
                }

                var seenOffsets = new HashSet<int>();
                var seenDates = new HashSet<DateOnly>();

                foreach (var point in forecast.Points)
                {
                    if (point.DayOffset < 1 || point.DayOffset > forecast.HorizonDays)
                    {
                        return $"Invalid DayOffset '{point.DayOffset}' for '{forecast.ModelType}'.";
                    }

                    if (!seenOffsets.Add(point.DayOffset))
                    {
                        return $"Duplicate DayOffset '{point.DayOffset}' for '{forecast.ModelType}'.";
                    }

                    if (!seenDates.Add(point.TargetDate))
                    {
                        return $"Duplicate TargetDate '{point.TargetDate}' for '{forecast.ModelType}'.";
                    }

                    var expectedDate = forecast.ForecastStartDate.AddDays(point.DayOffset - 1);
                    if (point.TargetDate != expectedDate)
                    {
                        return $"TargetDate '{point.TargetDate}' does not match DayOffset '{point.DayOffset}' for '{forecast.ModelType}'.";
                    }
                }

                var expectedEndDate = forecast.ForecastStartDate.AddDays(forecast.HorizonDays - 1);
                if (forecast.ForecastEndDate != expectedEndDate)
                {
                    return $"ForecastEndDate does not match ForecastStartDate + HorizonDays - 1 for '{forecast.ModelType}'.";
                }
            }

            var missingModelTypes = RequiredModelTypes
                .Where(x => !parsedModelTypes.Contains(x))
                .Select(x => x.ToString())
                .ToList();

            if (missingModelTypes.Count > 0)
            {
                return $"Missing model types: {string.Join(", ", missingModelTypes)}.";
            }

            return null;
        }

        private static string NormalizeSymbol(string symbol)
        {
            return symbol.Trim().ToUpperInvariant();
        }
    }
}