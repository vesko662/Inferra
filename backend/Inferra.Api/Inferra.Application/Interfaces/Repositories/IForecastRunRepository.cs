using Inferra.Domain.Entities;
using Inferra.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Repositories
{
    public interface IForecastRunRepository
    {
        Task<List<ForecastRun>> GetLatestByAssetIdAsync(int assetId);
        Task<List<ForecastRun>> GetLatestTrackedByAssetIdAndModelTypesAsync(int assetId, IEnumerable<ForecastModelType> modelTypes);
        Task AddRangeAsync(IEnumerable<ForecastRun> forecastRuns);
        Task SaveChangesAsync();
    }
}
