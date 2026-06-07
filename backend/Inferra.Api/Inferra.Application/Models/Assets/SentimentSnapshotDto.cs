namespace Inferra.Application.Models.Assets
{
    public class SentimentSnapshotDto
    {
        public int Bullish { get; set; }
        public int Bearish { get; set; }
        public int Neutral { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
