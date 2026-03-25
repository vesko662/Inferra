using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Domain.Entities
{
    public class MarketSnapshot
    {
        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public decimal Price { get; set; }
        public decimal? PriceChange24h { get; set; }
        public decimal? PriceChangePercentage24h { get; set; }

        public decimal Volume24h { get; set; }

        public decimal? MarketCap { get; set; }
        public decimal? FullyDilutedValuation { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
