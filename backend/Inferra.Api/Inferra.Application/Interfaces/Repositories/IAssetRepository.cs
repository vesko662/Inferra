using Inferra.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.Interfaces.Repositories
{
    public interface IAssetRepository
    {
        Task<bool> AnyAsync();
        Task<bool> ExistsBySymbolAsync(string symbol);
        Task<Asset?> GetByIdAsync(int id);
        Task<Asset?> GetBySymbolAsync(string symbol);
        Task<List<Asset>> GetAllAsync();
        Task AddAsync(Asset token);
        Task AddRangeAsync(IEnumerable<Asset> tokens);
        Task SaveChangesAsync();
    }
}
