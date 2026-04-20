using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Assets
{
    public class MarketSnapshotDto
    {
        public decimal Price { get; init; }
        public decimal? PriceChange24h { get; init; }
        public decimal? PriceChangePercentage24h { get; init; }
        public decimal Volume24h { get; init; }
        public decimal? MarketCap { get; init; }
        public decimal? FullyDilutedValuation { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
