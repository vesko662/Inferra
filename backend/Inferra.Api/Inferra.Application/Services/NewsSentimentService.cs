using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.News;
using Inferra.Domain.Entities;

namespace Inferra.Application.Services
{
    public class NewsSentimentService : INewsSentimentService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly INewsApiClient _newsApiClient;
        private readonly IMlClient _mlClient;
        private readonly ISentimentSnapshotRepository _sentimentSnapshotRepository;

        public NewsSentimentService(
            IAssetRepository assetRepository,
            INewsApiClient newsApiClient,
            IMlClient mlClient,
            ISentimentSnapshotRepository sentimentSnapshotRepository)
        {
            _assetRepository = assetRepository;
            _newsApiClient = newsApiClient;
            _mlClient = mlClient;
            _sentimentSnapshotRepository = sentimentSnapshotRepository;
        }

        public async Task<NewsSentimentResponse> GetSentimentAsync()
        {
            var assets = await _assetRepository.GetNewsAssetsAsync();
            var requests = new List<NewsSentimentRequest>(assets.Count);

            foreach (var asset in assets)
            {
                var news = await _newsApiClient.GetNewsAsync(asset.Symbol, asset.Name);

                if (news.Articles.Count == 0)
                {
                    continue;
                }

                var texts = news.Articles
                    .Select(a => string.IsNullOrWhiteSpace(a.Description)
                        ? a.Title
                        : $"{a.Title}. {a.Description}")
                    .ToList();

                requests.Add(new NewsSentimentRequest
                {
                    Symbol = asset.Symbol,
                    Texts = texts
                });
            }

            if (requests.Count == 0)
            {
                return new NewsSentimentResponse();
            }

            var results = await _mlClient.GetSentimentAsync(requests);

            var assetMap = assets.ToDictionary(x => x.Symbol, x => x.Id);
            var generatedAt = DateTime.UtcNow;

            foreach (var result in results)
            {
                if (!assetMap.TryGetValue(result.Symbol, out var assetId))
                {
                    continue;
                }

                var existing = await _sentimentSnapshotRepository.GetByAssetIdAsync(assetId);

                if (existing is null)
                {
                    await _sentimentSnapshotRepository.AddAsync(new SentimentSnapshot
                    {
                        AssetId = assetId,
                        Bullish = result.Bullish,
                        Bearish = result.Bearish,
                        Neutral = result.Neutral,
                        GeneratedAt = generatedAt
                    });
                }
                else
                {
                    existing.Bullish = result.Bullish;
                    existing.Bearish = result.Bearish;
                    existing.Neutral = result.Neutral;
                    existing.GeneratedAt = generatedAt;
                }
            }

            await _sentimentSnapshotRepository.SaveChangesAsync();

            return new NewsSentimentResponse { Results = results };
        }
    }
}
