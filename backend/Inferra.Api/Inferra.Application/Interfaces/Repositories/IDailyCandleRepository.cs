using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Repositories
{
    public interface IDailyCandleRepository
    {
        Task AddRangeAsync(IEnumerable<DailyCandle> candles);
        Task<bool> AnyAsync();
        Task<List<DailyCandle>> GetByAssetIdAsync(int assetId, DateOnly? from = null, DateOnly? to = null);
        Task<DateOnly?> GetLastDateByAssetIdAsync(int assetId);
        Task SaveChangesAsync();
    }
}
