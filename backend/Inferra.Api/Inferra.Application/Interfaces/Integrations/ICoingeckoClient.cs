using Inferra.Application.Models.Integrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Integrations
{
    public interface ICoingeckoClient
    {
        Task<IReadOnlyList<CoinGeckoAssetMetadataModel>> GetAssetsMetadataAsync(
            string vsCurrency = "usd",
            IEnumerable<string>? ids = null,
            IEnumerable<string>? symbols = null,
            int page = 1,
            int perPage = 250);
    }
}
