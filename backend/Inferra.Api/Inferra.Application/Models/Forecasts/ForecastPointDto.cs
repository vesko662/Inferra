using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Forecasts
{
    public class ForecastPointDto
    {
        public int DayOffset { get; init; }
        public DateOnly TargetDate { get; init; }
        public decimal PredictedPrice { get; init; }
    }
}
