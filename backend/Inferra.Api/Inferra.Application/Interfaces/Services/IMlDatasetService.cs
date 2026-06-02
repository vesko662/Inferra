using Inferra.Application.Models.Ml;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Services
{
    public interface IMlDatasetService
    {
        Task<TrainingDatasetResponse> GetTrainingDatasetAsync();
        Task<ForecastDatasetResponse> GetForecastDatasetAsync();
    }
}
