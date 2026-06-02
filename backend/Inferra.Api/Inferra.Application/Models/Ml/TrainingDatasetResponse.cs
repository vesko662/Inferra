using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Ml
{
    public class TrainingDatasetResponse
    {
        public DateOnly GeneratedOn { get; init; }
        public List<MlDatasetAssetDto> Assets { get; init; } = new();
    }
}
