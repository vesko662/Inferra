using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Ml;
using Inferra.Domain.Entities;


namespace Inferra.Application.Services
{
    public class MlDatasetService : IMlDatasetService
    {
        private const int ForecastLookbackDays = 120;

        private readonly IAssetRepository _assetRepository;
        private readonly IDailyCandleRepository _dailyCandleRepository;

        public MlDatasetService(
            IAssetRepository assetRepository,
            IDailyCandleRepository dailyCandleRepository)
        {
            _assetRepository = assetRepository;
            _dailyCandleRepository = dailyCandleRepository;
        }

        public async Task<TrainingDatasetResponse> GetTrainingDatasetAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var assets = await LoadAssetsAsync(fromDate: null, toDate: today);

            return new TrainingDatasetResponse
            {
                GeneratedOn = today,
                Assets = assets
            };
        }

        public async Task<ForecastDatasetResponse> GetForecastDatasetAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var fromDate = today.AddDays(-(ForecastLookbackDays - 1));
            var assets = await LoadAssetsAsync(fromDate, today);

            return new ForecastDatasetResponse
            {
                GeneratedOn = today,
                LookbackDays = ForecastLookbackDays,
                Assets = assets
            };
        }

        private async Task<List<MlDatasetAssetDto>> LoadAssetsAsync(DateOnly? fromDate, DateOnly toDate)
        {
            var selectedAssets = await _assetRepository.GetMlAssetsAsync();
            var result = new List<MlDatasetAssetDto>(selectedAssets.Count);

            foreach (var asset in selectedAssets)
            {
                var candles = await _dailyCandleRepository.GetByAssetIdAsync(asset.Id, fromDate, toDate);

                result.Add(new MlDatasetAssetDto
                {
                    Symbol = asset.Symbol,
                    Candles = candles
                        .OrderBy(x => x.Date)
                        .Select(MapCandle)
                        .ToList()
                });
            }

            return result;
        }

        private static MlDatasetCandleDto MapCandle(DailyCandle candle)
        {
            return new MlDatasetCandleDto
            {
                Date = candle.Date,
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.Volume
            };
        }
    }
}
