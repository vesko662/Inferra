using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Inferra.Domain.Entities
{
    public class Asset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Symbol { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string PairSymbol { get; set; } = null!;

        public string? CoinGeckoId { get; set; }
        public string? ImageUrl { get; set; }

        public decimal? CirculatingSupply { get; set; }
        public decimal? TotalSupply { get; set; }
        public decimal? MaxSupply { get; set; }

        public ICollection<DailyCandle> DailyCandles { get; set; } = new List<DailyCandle>();
        public MarketSnapshot? MarketSnapshot { get; set; }
    }
}
