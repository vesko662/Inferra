using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Forecasts
{
    public class SaveDailyForecastResult
    {
        public bool Success { get; init; }
        public bool AssetNotFound { get; init; }
        public string? ErrorMessage { get; init; }

        public static SaveDailyForecastResult Ok()
        {
            return new SaveDailyForecastResult
            {
                Success = true
            };
        }

        public static SaveDailyForecastResult NotFound()
        {
            return new SaveDailyForecastResult
            {
                AssetNotFound = true,
                ErrorMessage = "Asset was not found."
            };
        }

        public static SaveDailyForecastResult Invalid(string errorMessage)
        {
            return new SaveDailyForecastResult
            {
                ErrorMessage = errorMessage
            };
        }
    }
}
