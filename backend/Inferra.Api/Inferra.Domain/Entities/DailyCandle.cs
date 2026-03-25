using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Domain.Entities
{
    public class DailyCandle
    {
        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public DateOnly Date { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        public decimal Volume { get; set; }

    }
}
