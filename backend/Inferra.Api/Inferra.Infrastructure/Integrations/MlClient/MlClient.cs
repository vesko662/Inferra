using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Models.News;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Inferra.Infrastructure.Integrations.MlClient
{
    namespace Inferra.Infrastructure.Integrations.Ml
    {
        public class MlClient : IMlClient
        {
            private readonly HttpClient _httpClient;

            public MlClient(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task TriggerTrainingAsync(CancellationToken cancellationToken = default)
            {
                var response = await _httpClient.PostAsync("/train", null, cancellationToken);
                response.EnsureSuccessStatusCode();
            }

            public async Task TriggerDailyPredictionAsync(CancellationToken cancellationToken = default)
            {
                var response = await _httpClient.PostAsync("/predict-daily", null, cancellationToken);
                response.EnsureSuccessStatusCode();
            }

            public async Task<List<AssetSentimentDto>> GetSentimentAsync(List<NewsSentimentRequest> request, CancellationToken cancellationToken = default)
            {
                var response = await _httpClient.PostAsJsonAsync("/sentiment/predict", request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<List<AssetSentimentDto>>(cancellationToken);
                return result ?? new List<AssetSentimentDto>();
            }
        }
    }
}
