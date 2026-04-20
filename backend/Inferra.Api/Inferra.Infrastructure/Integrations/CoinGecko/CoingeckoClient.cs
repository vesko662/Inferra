using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Models.Integrations;
using System.Globalization;
using System.Text.Json;

public class CoingeckoClient : ICoingeckoClient
{
    private readonly HttpClient _httpClient;

    private const string CoinsMarketsEndpoint = "coins/markets";

    public CoingeckoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<CoinGeckoAssetMetadataModel>> GetAssetsMetadataAsync(
        string vsCurrency = "usd",
        IEnumerable<string>? ids = null,
        IEnumerable<string>? symbols = null,
        int page = 1,
        int perPage = 250)
    {
        if (string.IsNullOrWhiteSpace(vsCurrency))
        {
            throw new ArgumentException("vsCurrency is required.", nameof(vsCurrency));
        }

        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0.");
        }

        if (perPage < 1 || perPage > 250)
        {
            throw new ArgumentOutOfRangeException(nameof(perPage), "perPage must be between 1 and 250.");
        }

        var url = BuildCoinsMarketsUrl(vsCurrency, ids, symbols, page, perPage);

        using var response = await SendWithRetryAsync(url);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<CoinGeckoAssetMetadataModel>();
        }

        var result = new List<CoinGeckoAssetMetadataModel>();

        foreach (var item in document.RootElement.EnumerateArray())
        {
            result.Add(new CoinGeckoAssetMetadataModel
            {
                Id = GetString(item, "id") ?? string.Empty,
                Symbol = GetString(item, "symbol") ?? string.Empty,
                Name = GetString(item, "name") ?? string.Empty,
                ImageUrl = GetString(item, "image"),
                CirculatingSupply = GetNullableDecimal(item, "circulating_supply"),
                TotalSupply = GetNullableDecimal(item, "total_supply"),
                MaxSupply = GetNullableDecimal(item, "max_supply")
            });
        }

        return result;
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(string url)
    {
        const int maxAttempts = 4;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var response = await _httpClient.GetAsync(url);

            if ((int)response.StatusCode != 429)
            {
                response.EnsureSuccessStatusCode();
                return response;
            }

            response.Dispose();

            if (attempt == maxAttempts)
            {
                throw new HttpRequestException("CoinGecko rate limit reached (429 Too Many Requests).");
            }

            var delayMs = attempt switch
            {
                1 => 3000,
                2 => 7000,
                3 => 15000,
                _ => 30000
            };

            await Task.Delay(delayMs);
        }

        throw new InvalidOperationException("Unexpected retry flow.");
    }

    private static string BuildCoinsMarketsUrl(
        string vsCurrency,
        IEnumerable<string>? ids,
        IEnumerable<string>? symbols,
        int page,
        int perPage)
    {
        var queryParts = new List<string>
            {
                $"vs_currency={Uri.EscapeDataString(vsCurrency.Trim().ToLowerInvariant())}",
                "sparkline=false"
            };

        var normalizedIds = ids?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var normalizedSymbols = symbols?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedIds is { Length: > 0 })
        {
            queryParts.Add($"ids={Uri.EscapeDataString(string.Join(",", normalizedIds))}");
        }
        else if (normalizedSymbols is { Length: > 0 })
        {
            queryParts.Add($"symbols={Uri.EscapeDataString(string.Join(",", normalizedSymbols))}");
            queryParts.Add("include_tokens=top");
        }
        else
        {
            queryParts.Add($"page={page}");
            queryParts.Add($"per_page={perPage}");
        }

        return $"{CoinsMarketsEndpoint}?{string.Join("&", queryParts)}";
    }

    private static string? GetString(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            _ => null
        };
    }

    private static decimal? GetNullableDecimal(JsonElement item, string propertyName)
    {
        if (!item.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            if (value.TryGetDecimal(out var decimalValue))
            {
                return decimalValue;
            }

            if (value.TryGetDouble(out var doubleValue))
            {
                try
                {
                    return Convert.ToDecimal(doubleValue, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            var raw = value.GetString();

            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedDouble))
            {
                try
                {
                    return Convert.ToDecimal(parsedDouble, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return null;
                }
            }
        }

        return null;
    }
}