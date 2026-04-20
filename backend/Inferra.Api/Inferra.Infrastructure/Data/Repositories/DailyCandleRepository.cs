using Inferra.Application.Interfaces.Repositories;
using Inferra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.Data.Repositories
{
    public class DailyCandleRepository : IDailyCandleRepository
    {
        private readonly InferraDbContext _context;

        public DailyCandleRepository(InferraDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<DailyCandle> candles)
        {
            await _context.DailyCandles.AddRangeAsync(candles);
        }

        public Task<bool> AnyAsync()
        {
            return _context.DailyCandles.AnyAsync();
        }

        public Task<List<DailyCandle>> GetByAssetIdAsync(int assetId, DateOnly? from = null, DateOnly? to = null)
        {
            IQueryable<DailyCandle> query = _context.DailyCandles
                .AsNoTracking()
                .Where(dc => dc.AssetId == assetId);

            if (from.HasValue)
            {
                query = query.Where(dc => dc.Date >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(dc => dc.Date <= to.Value);
            }

            return query
                .OrderBy(dc => dc.Date)
                .ToListAsync();
        }

        public async Task<DateOnly?> GetLastDateByAssetIdAsync(int assetId)
        {
            return await _context.DailyCandles
                .AsNoTracking()
                .Where(dc => dc.AssetId == assetId)
                .OrderByDescending(dc => dc.Date)
                .Select(dc => (DateOnly?)dc.Date)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
