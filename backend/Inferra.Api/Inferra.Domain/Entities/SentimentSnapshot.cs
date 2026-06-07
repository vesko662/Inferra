namespace Inferra.Domain.Entities
{
    public class SentimentSnapshot
    {
        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public int Bullish { get; set; }
        public int Bearish { get; set; }
        public int Neutral { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
