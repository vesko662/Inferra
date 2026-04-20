using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Models.Integrations
{
    public class BinanceTradingPairModel
    {
        public string Symbol { get; set; } = null!;      
        public string BaseAsset { get; set; } = null!;   
        public string QuoteAsset { get; set; } = null!;  
        public string Status { get; set; } = null!;
    }
}
