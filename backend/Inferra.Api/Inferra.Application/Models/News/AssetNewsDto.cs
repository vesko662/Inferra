namespace Inferra.Application.Models.News
{
    public class AssetNewsDto
    {
        public string Symbol { get; set; } = null!;
        public List<NewsArticleDto> Articles { get; set; } = new();
    }
}
