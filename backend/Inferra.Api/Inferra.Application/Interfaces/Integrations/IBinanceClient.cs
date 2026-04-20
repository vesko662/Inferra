using Inferra.Application.Models.Integrations;
using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Integrations
{
    public interface IBinanceClient
    {
        public Task<IReadOnlyList<BinanceTradingPairModel>> GetTradingPairsAsync();

        public Task<IReadOnlyList<BinanceDailyCandleModel>> GetDailyCandlesBatchAsync(
         string pairSymbol,
         DateTimeOffset? startTimeUtc = null,
         DateTimeOffset? endTimeUtc = null,
         int limit = 1000);

        public  Task<IReadOnlyList<BinanceMarketSnapshotModel>> GetMarketSnapshotsAsync(
    IEnumerable<string> symbols,
    string type = "FULL");
    }
}
