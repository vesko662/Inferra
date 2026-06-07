using Inferra.Application.Interfaces.Repositories;
using Inferra.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inferra.Infrastructure.Data.Repositories
{
    public class SentimentSnapshotRepository : ISentimentSnapshotRepository
    {
        private readonly InferraDbContext _context;

        public SentimentSnapshotRepository(InferraDbContext context)
        {
            _context = context;
        }

        public Task<SentimentSnapshot?> GetByAssetIdAsync(int assetId)
        {
            return _context.SentimentSnapshots
                .FirstOrDefaultAsync(x => x.AssetId == assetId);
        }

        public async Task AddAsync(SentimentSnapshot snapshot)
        {
            await _context.SentimentSnapshots.AddAsync(snapshot);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
