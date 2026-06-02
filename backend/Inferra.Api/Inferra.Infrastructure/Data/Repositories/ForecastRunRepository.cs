using Inferra.Application.Interfaces.Repositories;
using Inferra.Domain.Entities;
using Inferra.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.Data.Repositories
{
    public class ForecastRunRepository : IForecastRunRepository
    {
        private readonly InferraDbContext _context;

        public ForecastRunRepository(InferraDbContext context)
        {
            _context = context;
        }

        public Task<List<ForecastRun>> GetLatestByAssetIdAsync(int assetId)
        {
            return _context.Set<ForecastRun>()
                .AsNoTracking()
                .Include(x => x.Points)
                .Where(x => x.AssetId == assetId && x.IsLatest)
                .OrderBy(x => x.ModelType)
                .ToListAsync();
        }

        public Task<List<ForecastRun>> GetLatestTrackedByAssetIdAndModelTypesAsync(int assetId, IEnumerable<ForecastModelType> modelTypes)
        {
            var modelTypeList = modelTypes.Distinct().ToList();

            return _context.Set<ForecastRun>()
                .Where(x => x.AssetId == assetId && x.IsLatest && modelTypeList.Contains(x.ModelType))
                .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<ForecastRun> forecastRuns)
        {
            await _context.Set<ForecastRun>().AddRangeAsync(forecastRuns);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
