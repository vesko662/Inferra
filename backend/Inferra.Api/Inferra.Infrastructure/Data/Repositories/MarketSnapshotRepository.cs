using Inferra.Application.Interfaces.Repositories;
using Inferra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.Data.Repositories
{
    public class MarketSnapshotRepository : IMarketSnapshotRepository
    {
        private readonly InferraDbContext _context;

        public MarketSnapshotRepository(InferraDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(MarketSnapshot snapshot)
        {
            await _context.MarketSnapshots.AddAsync(snapshot);
        }

        public async Task AddRangeAsync(IEnumerable<MarketSnapshot> snapshots)
        {
            await _context.MarketSnapshots.AddRangeAsync(snapshots);
        }

        public  Task<MarketSnapshot?> GetLatestByAssetIdAsync(int assetId)
        {
             return  _context.MarketSnapshots
                .Where(s => s.AssetId == assetId)
                .OrderByDescending(s => s.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        public Task<List<MarketSnapshot>> GetRangeByAssetIdAsync(int assetId, DateTime from, DateTime to)
        {
            return _context.MarketSnapshots
                .Where(s => s.AssetId == assetId && s.UpdatedAt >= from && s.UpdatedAt <= to)
                .OrderBy(s => s.UpdatedAt)
                .ToListAsync();
        }
        public async Task<MarketSnapshot?> GetByAssetIdAsync(int assetId)
        {
            return await _context.MarketSnapshots
                .FirstOrDefaultAsync(x => x.AssetId == assetId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
