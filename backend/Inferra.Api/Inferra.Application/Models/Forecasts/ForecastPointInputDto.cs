using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Forecasts
{
    public class ForecastPointInputDto
    {
        public int DayOffset { get; init; }

        public DateOnly TargetDate { get; init; }

        public decimal PredictedPrice { get; init; }
        public decimal? LowerBound { get; init; }
        public decimal? UpperBound { get; init; }
        public decimal? Confidence { get; init; }
    }
}
