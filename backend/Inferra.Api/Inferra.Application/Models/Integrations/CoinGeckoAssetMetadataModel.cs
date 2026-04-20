using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Inferra.Application.Models.Integrations
{
    public class CoinGeckoAssetMetadataModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("circulating_supply")]
        public decimal? CirculatingSupply { get; set; }

        [JsonPropertyName("total_supply")]
        public decimal? TotalSupply { get; set; }

        [JsonPropertyName("max_supply")]
        public decimal? MaxSupply { get; set; }
    }
}
