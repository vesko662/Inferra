using Inferra.Application.Models.Forecasts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Services
{
    public interface IForecastIngestionService
    {
        Task<SaveDailyForecastResult> SaveDailyForecastAsync(SaveDailyForecastRequest request);
    }
}
