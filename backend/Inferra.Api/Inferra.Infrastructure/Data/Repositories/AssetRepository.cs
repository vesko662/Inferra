using Inferra.Application.Interfaces.Repositories;
using Inferra.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Inferra.Infrastructure.Data.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly InferraDbContext _context;
        public AssetRepository(InferraDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Asset token)
        {
            await _context.Assets.AddAsync(token);
        }

        public async Task AddRangeAsync(IEnumerable<Asset> tokens)
        {
            await _context.Assets.AddRangeAsync(tokens);
        }

        public async Task<bool> AnyAsync()
        {
            return await _context.Assets.AnyAsync();
        }

        public Task<bool> ExistsBySymbolAsync(string symbol)
        {
            return _context.Assets.AnyAsync(a => a.Symbol == symbol);
        }

        public Task<List<Asset>> GetAllAsync()
        {
            return _context.Assets.ToListAsync();   
        }

        public Task<Asset?> GetByIdAsync(int id)
        {
            return _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<Asset?> GetBySymbolAsync(string symbol)
        {
            return _context.Assets.FirstOrDefaultAsync(a => a.Symbol == symbol);
        }
        public Task<List<Asset>> GetTopByHistoryLengthAsync(int count)
        {
            return _context.Assets
                .AsNoTracking()
                .Where(a => a.DailyCandles.Any())
                .OrderByDescending(a => a.DailyCandles.Count)
                .ThenBy(a => a.Symbol)
                .Take(count)
                .ToListAsync();
        }

        public Task<List<Asset>> GetMlAssetsAsync()
        {
            return _context.Assets
                .AsNoTracking()
                .Where(x => x.IsMlEnabled)
                .OrderBy(x => x.Symbol)
                .ToListAsync();
        }

        public Task<List<Asset>> GetNewsAssetsAsync()
        {
            return _context.Assets
                .AsNoTracking()
                .Where(x => x.IsNewsEnabled)
                .OrderBy(x => x.Symbol)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
