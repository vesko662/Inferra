using Inferra.Domain.Entities;

namespace Inferra.Application.Interfaces.Repositories
{
    public interface ISentimentSnapshotRepository
    {
        Task<SentimentSnapshot?> GetByAssetIdAsync(int assetId);
        Task AddAsync(SentimentSnapshot snapshot);
        Task SaveChangesAsync();
    }
}
