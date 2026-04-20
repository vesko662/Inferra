using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Assets;
using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Services
{
    public class AssetQueryService : IAssetQueryService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IDailyCandleRepository _dailyCandleRepository;
        private readonly IMarketSnapshotRepository _marketSnapshotRepository;

        public AssetQueryService(
            IAssetRepository assetRepository,
            IDailyCandleRepository dailyCandleRepository,
            IMarketSnapshotRepository marketSnapshotRepository)
        {
            _assetRepository = assetRepository;
            _dailyCandleRepository = dailyCandleRepository;
            _marketSnapshotRepository = marketSnapshotRepository;
        }

        public async Task<IReadOnlyList<AssetListItemDto>> GetAssetsAsync()
        {
            var assets = await _assetRepository.GetAllAsync();

            return assets
                .Select(MapAssetListItem)
                .ToList();
        }

        public async Task<AssetDetailsDto?> GetAssetBySymbolAsync(string symbol)
        {
            var asset = await _assetRepository.GetBySymbolAsync(NormalizeSymbol(symbol));
            if (asset is null)
            {
                return null;
            }

            var snapshot = await _marketSnapshotRepository.GetLatestByAssetIdAsync(asset.Id);

            return MapAssetDetails(asset, snapshot);
        }

        public async Task<MarketSnapshotDto?> GetLatestSnapshotBySymbolAsync(string symbol)
        {
            var asset = await _assetRepository.GetBySymbolAsync(NormalizeSymbol(symbol));
            if (asset is null)
            {
                return null;
            }

            var snapshot = await _marketSnapshotRepository.GetLatestByAssetIdAsync(asset.Id);
            if (snapshot is null)
            {
                return null;
            }

            return MapSnapshot(snapshot);
        }

        public async Task<IReadOnlyList<DailyCandleDto>?> GetCandlesBySymbolAsync(string symbol, DateOnly? from, DateOnly? to)
        {
            var asset = await _assetRepository.GetBySymbolAsync(NormalizeSymbol(symbol));
            if (asset is null)
            {
                return null;
            }

            var candles = await _dailyCandleRepository.GetByAssetIdAsync(asset.Id, from, to);

            return candles
                .Select(MapCandle)
                .ToList();
        }

        private static string NormalizeSymbol(string symbol)
        {
            return symbol.Trim().ToUpperInvariant();
        }

        private static AssetListItemDto MapAssetListItem(Asset asset)
        {
            return new AssetListItemDto
            {
                Id = asset.Id,
                Symbol = asset.Symbol,
                Name = asset.Name,
                PairSymbol = asset.PairSymbol,
                ImageUrl = asset.ImageUrl
            };
        }

        private static AssetDetailsDto MapAssetDetails(Asset asset, MarketSnapshot? snapshot)
        {
            return new AssetDetailsDto
            {
                Id = asset.Id,
                Symbol = asset.Symbol,
                Name = asset.Name,
                PairSymbol = asset.PairSymbol,
                CoinGeckoId = asset.CoinGeckoId,
                ImageUrl = asset.ImageUrl,
                CirculatingSupply = asset.CirculatingSupply,
                TotalSupply = asset.TotalSupply,
                MaxSupply = asset.MaxSupply,
                Snapshot = snapshot is null ? null : MapSnapshot(snapshot)
            };
        }

        private static MarketSnapshotDto MapSnapshot(MarketSnapshot snapshot)
        {
            return new MarketSnapshotDto
            {
                Price = snapshot.Price,
                PriceChange24h = snapshot.PriceChange24h,
                PriceChangePercentage24h = snapshot.PriceChangePercentage24h,
                Volume24h = snapshot.Volume24h,
                MarketCap = snapshot.MarketCap,
                FullyDilutedValuation = snapshot.FullyDilutedValuation,
                UpdatedAt = snapshot.UpdatedAt
            };
        }

        private static DailyCandleDto MapCandle(DailyCandle candle)
        {
            return new DailyCandleDto
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