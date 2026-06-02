using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Forecasts
{
    public class AssetLatestForecastDto
    {
        public string Symbol { get; init; } = null!;
        public List<ModelForecastDto> Forecasts { get; init; } = new();
    }
}
