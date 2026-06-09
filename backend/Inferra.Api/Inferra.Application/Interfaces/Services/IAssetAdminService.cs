namespace Inferra.Application.Interfaces.Services
{
    public interface IAssetAdminService
    {
        Task<bool> SetMlEnabledAsync(string symbol, bool enabled);
        Task<bool> SetNewsEnabledAsync(string symbol, bool enabled);
        Task<bool> DeleteAsync(string symbol);
    }
}
