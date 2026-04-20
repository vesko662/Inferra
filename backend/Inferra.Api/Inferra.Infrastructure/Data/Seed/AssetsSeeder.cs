// File: Inferra.Infrastructure/Data/Seed/AssetsSeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inferra.Application.Interfaces;
using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Models.Integrations;
using Inferra.Domain.Entities;

namespace Inferra.Infrastructure.Data.Seed
{
    public sealed class AssetsSeeder
    {
        private const int CoinGeckoSymbolsPerRequest = 50;
        private const int CoinGeckoRequestDelayMs = 2500;
        private const int BinanceBatchSize = 1000;
        private const string QuoteAssetUsdt = "USDT";
        private const string CoinGeckoVsCurrency = "usd";

        private readonly IBinanceClient _binanceClient;
        private readonly ICoingeckoClient _coingeckoClient;
        private readonly IAssetRepository _assetRepository;
        private readonly IDailyCandleRepository _dailyCandleRepository;

        public AssetsSeeder(
            IBinanceClient binanceClient,
            ICoingeckoClient coingeckoClient,
            IAssetRepository assetRepository,
            IDailyCandleRepository dailyCandleRepository)
        {
            _binanceClient = binanceClient;
            _coingeckoClient = coingeckoClient;
            _assetRepository = assetRepository;
            _dailyCandleRepository = dailyCandleRepository;
        }

        public async Task SeedAsync()
        {
            await SeedAssetsAsync();
            await SeedDailyCandlesAsync();
        }

        private async Task SeedAssetsAsync()
        {
            if (await _assetRepository.AnyAsync())
            {
                return;
            }

            var tradingPairs = await _binanceClient.GetTradingPairsAsync();

            var pairsByBaseSymbol = tradingPairs
                .Where(IsSupportedPair)
                .GroupBy(pair => Normalize(pair.BaseAsset), StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrEmpty(group.Key))
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);

            if (pairsByBaseSymbol.Count == 0)
            {
                return;
            }

            var coinGeckoMatches = await GetCoinGeckoTokensForBinancePairsAsync(pairsByBaseSymbol.Values.ToList());
            var assets = BuildAssets(pairsByBaseSymbol, coinGeckoMatches);

            if (assets.Count == 0)
            {
                return;
            }

            await _assetRepository.AddRangeAsync(assets);
            await _assetRepository.SaveChangesAsync();
        }

        private async Task SeedDailyCandlesAsync()
        {
            var assets = (await _assetRepository.GetAllAsync())
                .Where(asset => !string.IsNullOrWhiteSpace(asset.PairSymbol))
                .OrderBy(asset => asset.Symbol)
                .ToList();

            foreach (var asset in assets)
            {
                await SeedDailyCandlesForAssetAsync(asset);
            }
        }

        private async Task SeedDailyCandlesForAssetAsync(Asset asset)
        {
            var lastSavedDate = await _dailyCandleRepository.GetLastDateByAssetIdAsync(asset.Id);
            var startTimeUtc = BuildStartTimeUtc(lastSavedDate);

            while (true)
            {
                var batch = await _binanceClient.GetDailyCandlesBatchAsync(
                    pairSymbol: asset.PairSymbol,
                    startTimeUtc: startTimeUtc,
                    endTimeUtc: null,
                    limit: BinanceBatchSize);

                if (batch.Count == 0)
                {
                    break;
                }

                var candlesToInsert = MapNewDailyCandles(asset.Id, batch, lastSavedDate, DateTimeOffset.UtcNow);

                if (candlesToInsert.Count > 0)
                {
                    await _dailyCandleRepository.AddRangeAsync(candlesToInsert);
                    await _dailyCandleRepository.SaveChangesAsync();
                    lastSavedDate = candlesToInsert.Max(candle => candle.Date);
                }

                if (batch.Count < BinanceBatchSize)
                {
                    break;
                }

                var nextStartTimeUtc = batch.Max(candle => candle.OpenTimeUtc).AddDays(1);

                if (nextStartTimeUtc <= startTimeUtc)
                {
                    break;
                }

                startTimeUtc = nextStartTimeUtc;
            }
        }

        private async Task<Dictionary<string, CoinGeckoAssetMetadataModel>> GetCoinGeckoTokensForBinancePairsAsync(
            IReadOnlyCollection<BinanceTradingPairModel> binancePairs)
        {
            var symbols = binancePairs
                .Select(pair => Normalize(pair.BaseAsset))
                .Where(symbol => !string.IsNullOrEmpty(symbol))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var result = new Dictionary<string, CoinGeckoAssetMetadataModel>(StringComparer.OrdinalIgnoreCase);

            foreach (var chunk in symbols.Chunk(CoinGeckoSymbolsPerRequest))
            {
                var tokens = await _coingeckoClient.GetAssetsMetadataAsync(
                    vsCurrency: CoinGeckoVsCurrency,
                    ids: null,
                    symbols: chunk,
                    page: 1,
                    perPage: 250);

                foreach (var token in tokens)
                {
                    var symbol = Normalize(token.Symbol);

                    if (string.IsNullOrEmpty(symbol) || result.ContainsKey(symbol))
                    {
                        continue;
                    }

                    result[symbol] = token;
                }

                if (chunk != symbols.Chunk(CoinGeckoSymbolsPerRequest).Last())
                {
                    await Task.Delay(CoinGeckoRequestDelayMs);
                }
            }

            return result;
        }

        private static List<Asset> BuildAssets(
            IReadOnlyDictionary<string, BinanceTradingPairModel> pairsByBaseSymbol,
            IReadOnlyDictionary<string, CoinGeckoAssetMetadataModel> coinGeckoMatches)
        {
            var assets = new List<Asset>(coinGeckoMatches.Count);

            foreach (var (symbol, metadata) in coinGeckoMatches)
            {
                if (!pairsByBaseSymbol.TryGetValue(symbol, out var binancePair))
                {
                    continue;
                }

                assets.Add(new Asset
                {
                    Symbol = symbol,
                    Name = string.IsNullOrWhiteSpace(metadata.Name) ? symbol : metadata.Name.Trim(),
                    PairSymbol = Normalize(binancePair.Symbol),
                    CoinGeckoId = metadata.Id,
                    ImageUrl = metadata.ImageUrl,
                    CirculatingSupply = metadata.CirculatingSupply,
                    TotalSupply = metadata.TotalSupply,
                    MaxSupply = metadata.MaxSupply
                });
            }

            return assets;
        }

        private static List<DailyCandle> MapNewDailyCandles(
            int assetId,
            IReadOnlyCollection<BinanceDailyCandleModel> batch,
            DateOnly? lastSavedDate,
            DateTimeOffset nowUtc)
        {
            var candles = new List<DailyCandle>(batch.Count);

            foreach (var candle in batch.OrderBy(item => item.OpenTimeUtc))
            {
                if (candle.CloseTimeUtc > nowUtc)
                {
                    continue;
                }

                if (lastSavedDate.HasValue && candle.Date <= lastSavedDate.Value)
                {
                    continue;
                }

                candles.Add(new DailyCandle
                {
                    AssetId = assetId,
                    Date = candle.Date,
                    Open = candle.Open,
                    High = candle.High,
                    Low = candle.Low,
                    Close = candle.Close,
                    Volume = candle.Volume
                });
            }

            return candles;
        }

        private static bool IsSupportedPair(BinanceTradingPairModel pair)
        {
            return Normalize(pair.QuoteAsset) == QuoteAssetUsdt
                && !string.IsNullOrEmpty(Normalize(pair.BaseAsset))
                && !string.IsNullOrEmpty(Normalize(pair.Symbol));
        }

        private static DateTimeOffset BuildStartTimeUtc(DateOnly? lastSavedDate)
        {
            if (!lastSavedDate.HasValue)
            {
                return DateTimeOffset.UnixEpoch;
            }

            var nextDateUtc = DateTime.SpecifyKind(
                lastSavedDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            return new DateTimeOffset(nextDateUtc);
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }
    }
}