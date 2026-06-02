using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Domain.Entities
{
    [Index(nameof(ForecastRunId), nameof(DayOffset), IsUnique = true)]
    [Index(nameof(ForecastRunId), nameof(TargetDate), IsUnique = true)]
    public class ForecastPoint
    {
        public int Id { get; set; }

        public int ForecastRunId { get; set; }
        public ForecastRun ForecastRun { get; set; } = null!;

        public int DayOffset { get; set; }

        public DateOnly TargetDate { get; set; }

        [Precision(18, 8)]
        public decimal PredictedPrice { get; set; }

        [Precision(18, 8)]
        public decimal? LowerBound { get; set; }

        [Precision(18, 8)]
        public decimal? UpperBound { get; set; }

        [Precision(10, 6)]
        public decimal? Confidence { get; set; }
    }
}
