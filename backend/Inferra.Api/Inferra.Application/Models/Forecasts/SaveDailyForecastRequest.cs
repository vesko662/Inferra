using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Inferra.Application.Models.Forecasts
{
    public class SaveDailyForecastRequest
    {
        public string Symbol { get; init; } = null!;

        public DateTime GeneratedAt { get; init; }

        public List<ModelForecastInputDto> Forecasts { get; init; } = new();
    }
}
