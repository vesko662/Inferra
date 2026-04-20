using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Assets
{
    public class AssetDetailsDto
    {
        public int Id { get; init; }
        public string Symbol { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string PairSymbol { get; init; } = null!;
        public string? CoinGeckoId { get; init; }
        public string? ImageUrl { get; init; }
        public decimal? CirculatingSupply { get; init; }
        public decimal? TotalSupply { get; init; }
        public decimal? MaxSupply { get; init; }
        public MarketSnapshotDto? Snapshot { get; init; }
    }
}
