namespace Inferra.Application.Models.News
{
    public class AssetSentimentDto
    {
        public string Symbol { get; set; } = null!;
        public int Bullish { get; set; }
        public int Bearish { get; set; }
        public int Neutral { get; set; }
    }
}
