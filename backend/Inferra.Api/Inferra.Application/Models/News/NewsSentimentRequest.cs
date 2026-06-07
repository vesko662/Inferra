namespace Inferra.Application.Models.News
{
    public class NewsSentimentRequest
    {
        public string Symbol { get; set; } = null!;
        public List<string> Texts { get; set; } = new();
    }
}
