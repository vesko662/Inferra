using Inferra.Application.Interfaces.Repositories;
using Inferra.Application.Interfaces.Services;

namespace Inferra.Application.Services
{
    public class AssetAdminService : IAssetAdminService
    {
        private readonly IAssetRepository _assetRepository;

        public AssetAdminService(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public Task<bool> SetMlEnabledAsync(string symbol, bool enabled)
            => _assetRepository.SetMlEnabledAsync(symbol.Trim().ToUpperInvariant(), enabled);

        public Task<bool> SetNewsEnabledAsync(string symbol, bool enabled)
            => _assetRepository.SetNewsEnabledAsync(symbol.Trim().ToUpperInvariant(), enabled);

        public Task<bool> DeleteAsync(string symbol)
            => _assetRepository.SoftDeleteAsync(symbol.Trim().ToUpperInvariant());
    }
}
