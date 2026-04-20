using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Integrations
{
    public class BinanceDailyCandleModel
    {
        public DateOnly Date { get; set; }

        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        public decimal Volume { get; set; }

        public DateTimeOffset OpenTimeUtc { get; set; }
        public DateTimeOffset CloseTimeUtc { get; set; }
    }
}
