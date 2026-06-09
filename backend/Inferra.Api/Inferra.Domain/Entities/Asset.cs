using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

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
        [Precision(38, 8)]
        public decimal? CirculatingSupply { get; set; }
        [Precision(38, 8)]
        public decimal? TotalSupply { get; set; }
        [Precision(38, 8)]
        public decimal? MaxSupply { get; set; }

        public bool IsMlEnabled { get; set; }
        public bool IsNewsEnabled { get; set; }
        public bool IsDeleted { get; set; }

        public ICollection<DailyCandle> DailyCandles { get; set; } = new List<DailyCandle>();
        public ICollection<ForecastRun> ForecastRuns { get; set; } = new List<ForecastRun>();

        public MarketSnapshot? MarketSnapshot { get; set; }
        public SentimentSnapshot? SentimentSnapshot { get; set; }
    }
}
