using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Ml
{
    public class MlDatasetCandleDto
    {
        public DateOnly Date { get; init; }
        public decimal Open { get; init; }
        public decimal High { get; init; }
        public decimal Low { get; init; }
        public decimal Close { get; init; }
        public decimal Volume { get; init; }
    }
}
