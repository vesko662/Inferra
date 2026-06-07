using Inferra.Application.Models.News;

namespace Inferra.Application.Interfaces.Integrations
{
    public interface IMlClient
    {
        Task TriggerTrainingAsync(CancellationToken cancellationToken = default);
        Task TriggerDailyPredictionAsync(CancellationToken cancellationToken = default);
        Task<List<AssetSentimentDto>> GetSentimentAsync(List<NewsSentimentRequest> request, CancellationToken cancellationToken = default);
    }
}
