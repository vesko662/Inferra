using Inferra.Application.Interfaces.Integrations;
using System;
using System.Collections.Generic;
using System.Text;

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
                var response = await _httpClient.PostAsync(
                    "/train",
                    null,
                    cancellationToken);

                response.EnsureSuccessStatusCode();
            }

            public async Task TriggerDailyPredictionAsync(CancellationToken cancellationToken = default)
            {
                var response = await _httpClient.PostAsync(
                    "/predict-daily",
                    null,
                    cancellationToken);

                response.EnsureSuccessStatusCode();
            }
        }
    }
}
