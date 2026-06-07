namespace Inferra.Application.Models.News
{
    public class NewsSentimentResponse
    {
        public List<AssetSentimentDto> Results { get; set; } = new();
    }
}
