using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Repositories
{
    public interface IMarketSnapshotRepository
    {
        Task AddAsync(MarketSnapshot snapshot);
        Task AddRangeAsync(IEnumerable<MarketSnapshot> snapshots);

        Task<MarketSnapshot?> GetLatestByAssetIdAsync(int assetId);

        Task<List<MarketSnapshot>> GetRangeByAssetIdAsync(
            int assetId,
            DateTime from,
            DateTime to);

        Task SaveChangesAsync();

        Task<MarketSnapshot?> GetByAssetIdAsync(int assetId);
    }
}
