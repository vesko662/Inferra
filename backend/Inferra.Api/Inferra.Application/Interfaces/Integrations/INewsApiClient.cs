using Inferra.Application.Models.News;

namespace Inferra.Application.Interfaces.Integrations
{
    public interface INewsApiClient
    {
        Task<AssetNewsDto> GetNewsAsync(string symbol, string name);
    }
}
