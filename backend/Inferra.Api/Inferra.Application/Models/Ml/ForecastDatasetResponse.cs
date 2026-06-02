using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Ml
{
    public class ForecastDatasetResponse
    {
        public DateOnly GeneratedOn { get; init; }
        public int LookbackDays { get; init; }
        public List<MlDatasetAssetDto> Assets { get; init; } = new();
    }
}
