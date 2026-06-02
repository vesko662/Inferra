using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Forecasts
{
    public class ModelForecastDto
    {
        public string ModelType { get; init; } = null!;
        public string ModelVersion { get; init; } = null!;
        public DateTime GeneratedAt { get; init; }
        public DateOnly ForecastStartDate { get; init; }
        public DateOnly ForecastEndDate { get; init; }
        public int HorizonDays { get; init; }
        public List<ForecastPointDto> Points { get; init; } = new();
    }
}
