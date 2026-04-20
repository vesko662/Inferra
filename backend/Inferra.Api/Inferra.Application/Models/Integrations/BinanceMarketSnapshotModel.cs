using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Inferra.Application.Models.Integrations
{
    public class BinanceMarketSnapshotModel
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("volume")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Volume { get; set; }

        [JsonPropertyName("priceChange")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal PriceChange { get; set; }

        [JsonPropertyName("priceChangePercent")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal PriceChangePercent { get; set; }

        [JsonPropertyName("lastPrice")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Price { get; set; }

    }
}
