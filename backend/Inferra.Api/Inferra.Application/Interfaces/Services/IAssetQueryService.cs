using Inferra.Application.Models.Assets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Services
{
    public interface IAssetQueryService
    {
        Task<IReadOnlyList<AssetListItemDto>> GetAssetsAsync();
        Task<AssetDetailsDto?> GetAssetBySymbolAsync(string symbol);
        Task<MarketSnapshotDto?> GetLatestSnapshotBySymbolAsync(string symbol);
        Task<IReadOnlyList<DailyCandleDto>?> GetCandlesBySymbolAsync(string symbol, DateOnly? from, DateOnly? to);
    }
}
