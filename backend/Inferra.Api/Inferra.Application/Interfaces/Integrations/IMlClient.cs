using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Integrations
{
    public interface IMlClient
    {
        Task TriggerTrainingAsync(CancellationToken cancellationToken = default);
        Task TriggerDailyPredictionAsync(CancellationToken cancellationToken = default);
    }
}
