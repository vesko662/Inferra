using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Models.News;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inferra.Infrastructure.Integrations.NewsApi
{
    public class InferraNewsApiClient : INewsApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private const string EverythingEndpoint = "everything";

        public InferraNewsApiClient(HttpClient httpClient, string apiKey)
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
        }

        public async Task<AssetNewsDto> GetNewsAsync(string symbol, string name)
        {
            var to = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
            var from = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd");
            var url = $"{EverythingEndpoint}?q={Uri.EscapeDataString(name)}&from={from}&to={to}&sortBy=popularity&apiKey={_apiKey}";

            using var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"NewsAPI {(int)response.StatusCode}: {errorBody}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var payload = await JsonSerializer.DeserializeAsync<NewsApiResponse>(stream, JsonOptions);

            var articles = new List<NewsArticleDto>();

            if (payload?.Status == "ok" && payload.Articles is not null)
            {
                foreach (var article in payload.Articles)
                {
                    if (string.IsNullOrWhiteSpace(article.Title))
                    {
                        continue;
                    }

                    articles.Add(new NewsArticleDto
                    {
                        Title = article.Title,
                        Description = article.Description
                    });
                }
            }

            return new AssetNewsDto
            {
                Symbol = symbol,
                Articles = articles
            };
        }

        private sealed class NewsApiResponse
        {
            public string? Status { get; set; }
            public List<NewsApiArticle>? Articles { get; set; }
        }

        private sealed class NewsApiArticle
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
        }
    }
}
