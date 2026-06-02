using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Forecasts;
using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Services
{
    public class ForecastQueryService : IForecastQueryService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IForecastRunRepository _forecastRunRepository;

        public ForecastQueryService(
            IAssetRepository assetRepository,
            IForecastRunRepository forecastRunRepository)
        {
            _assetRepository = assetRepository;
            _forecastRunRepository = forecastRunRepository;
        }

        public async Task<AssetLatestForecastDto?> GetLatestBySymbolAsync(string symbol)
        {
            var normalizedSymbol = NormalizeSymbol(symbol);
            var asset = await _assetRepository.GetBySymbolAsync(normalizedSymbol);

            if (asset is null)
            {
                return null;
            }

            var runs = await _forecastRunRepository.GetLatestByAssetIdAsync(asset.Id);

            return new AssetLatestForecastDto
            {
                Symbol = asset.Symbol,
                Forecasts = runs
                    .OrderBy(x => x.ModelType)
                    .Select(MapRun)
                    .ToList()
            };
        }

        private static ModelForecastDto MapRun(ForecastRun forecastRun)
        {
            return new ModelForecastDto
            {
                ModelType = forecastRun.ModelType.ToString(),
                ModelVersion = forecastRun.ModelVersion,
                GeneratedAt = forecastRun.GeneratedAt,
                ForecastStartDate = forecastRun.ForecastStartDate,
                ForecastEndDate = forecastRun.ForecastEndDate,
                HorizonDays = forecastRun.HorizonDays,
                Points = forecastRun.Points
                    .OrderBy(x => x.DayOffset)
                    .Select(x => new ForecastPointDto
                    {
                        DayOffset = x.DayOffset,
                        TargetDate = x.TargetDate,
                        PredictedPrice = x.PredictedPrice,
                    })
                    .ToList()
            };
        }

        private static string NormalizeSymbol(string symbol)
        {
            return symbol.Trim().ToUpperInvariant();
        }
    }
}
