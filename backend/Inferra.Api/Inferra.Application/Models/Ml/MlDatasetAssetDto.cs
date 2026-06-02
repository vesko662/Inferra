using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Ml
{
    public class MlDatasetAssetDto
    {
        public string Symbol { get; init; } = null!;
        public List<MlDatasetCandleDto> Candles { get; init; } = new();
    }
}
