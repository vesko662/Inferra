using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Models.Integrations;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inferra.Infrastructure.Integrations.Binance
{
    public class BinanceClient:IBinanceClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;

        private const string ExchangeInfoEndpoint = "api/v3/exchangeInfo";
        private const string KlinesEndpoint = "api/v3/klines";
        private const string MarketShapshotEndpoint = "api/v3/ticker/24hr";
        private const string Interval = "1d";

        public BinanceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<BinanceTradingPairModel>> GetTradingPairsAsync()
        {
            using var response = await _httpClient.GetAsync(ExchangeInfoEndpoint);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<ExchangeInfoResponse>(stream, JsonOptions);
            
            if (payload?.Symbols is null || payload.Symbols.Count == 0)
            {
                return Array.Empty<BinanceTradingPairModel>();
            }

            return payload.Symbols;
        }

        public async Task<IReadOnlyList<BinanceDailyCandleModel>> GetDailyCandlesBatchAsync(
         string pairSymbol,
         DateTimeOffset? startTimeUtc = null,
         DateTimeOffset? endTimeUtc = null,
         int limit = 1000)
        {
            if (string.IsNullOrWhiteSpace(pairSymbol))
            {
                throw new ArgumentException("Pair symbol is required.", nameof(pairSymbol));
            }

            if (limit < 1 || limit > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be between 1 and 1000.");
            }

            if (startTimeUtc.HasValue && endTimeUtc.HasValue && startTimeUtc > endTimeUtc)
            {
                throw new ArgumentException("startTimeUtc cannot be greater than endTimeUtc.");
            }

            var url = BuildKlinesUrl(pairSymbol, startTimeUtc, endTimeUtc, limit);

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<List<List<JsonElement>>>(stream, JsonOptions);

            if (payload is null || payload.Count == 0)
            {
                return Array.Empty<BinanceDailyCandleModel>();
            }

            var candles = new List<BinanceDailyCandleModel>(payload.Count);

            foreach (var row in payload)
            {
                if (row.Count < 7)
                {
                    continue;
                }

                var openTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(row[0].GetInt64());
                var closeTimeUtc = DateTimeOffset.FromUnixTimeMilliseconds(row[6].GetInt64());

                candles.Add(new BinanceDailyCandleModel
                {
                    Date = DateOnly.FromDateTime(openTimeUtc.UtcDateTime),
                    OpenTimeUtc = openTimeUtc,
                    CloseTimeUtc = closeTimeUtc,
                    Open = ParseDecimal(row[1]),
                    High = ParseDecimal(row[2]),
                    Low = ParseDecimal(row[3]),
                    Close = ParseDecimal(row[4]),
                    Volume = ParseDecimal(row[5])
                });
            }

            return candles;
        }

        private static string BuildKlinesUrl(
            string pairSymbol,
            DateTimeOffset? startTimeUtc,
            DateTimeOffset? endTimeUtc,
            int limit)
        {
            var queryParts = new List<string>
            {
                $"symbol={Uri.EscapeDataString(pairSymbol.Trim().ToUpperInvariant())}",
                $"interval={Interval}",
                $"limit={limit}"
            };

            if (startTimeUtc.HasValue)
            {
                queryParts.Add($"startTime={startTimeUtc.Value.ToUnixTimeMilliseconds()}");
            }

            if (endTimeUtc.HasValue)
            {
                queryParts.Add($"endTime={endTimeUtc.Value.ToUnixTimeMilliseconds()}");
            }

            return $"{KlinesEndpoint}?{string.Join("&", queryParts)}";
        }

        private static decimal ParseDecimal(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => decimal.Parse(
                    element.GetString() ?? "0",
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture),

                JsonValueKind.Number => element.GetDecimal(),

                _ => 0m
            };
        }

        public async Task<IReadOnlyList<BinanceMarketSnapshotModel>> GetMarketSnapshotsAsync(
     IEnumerable<string> symbols,
     string type = "FULL")
        {
            if (symbols is null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            var normalizedSymbols = symbols
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();

            if (normalizedSymbols.Count == 0)
            {
                return Array.Empty<BinanceMarketSnapshotModel>();
            }

            var symbolsJson = JsonSerializer.Serialize(normalizedSymbols);

            var url =
                $"{MarketShapshotEndpoint}?symbols={Uri.EscapeDataString(symbolsJson)}&type={Uri.EscapeDataString(type)}";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<List<BinanceMarketSnapshotModel>>(stream, JsonOptions);

            return payload ?? new List<BinanceMarketSnapshotModel>();
        }

        private sealed class ExchangeInfoResponse
        {
            [JsonPropertyName("symbols")]
            public List<BinanceTradingPairModel> Symbols { get; set; } = new();
        }
    }
}
