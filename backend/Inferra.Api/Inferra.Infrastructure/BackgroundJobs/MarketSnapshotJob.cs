using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Interfaces.Repositories;
using Inferra.Domain.Entities;
using Inferra.Infrastructure.Data.Repositories;
using Inferra.Infrastructure.Integrations.Binance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.BackgroundJobs
{
    public class MarketSnapshotJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MarketSnapshotJob> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public MarketSnapshotJob(
            IServiceScopeFactory scopeFactory,
            ILogger<MarketSnapshotJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MarketSnapshotJob started.");

            await RunOnceAsync(stoppingToken);

            using var timer = new PeriodicTimer(_interval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunOnceAsync(stoppingToken);
            }
        }

        private async Task RunOnceAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var binance = scope.ServiceProvider.GetRequiredService<IBinanceClient>();
                var marketSnapshotRepository = scope.ServiceProvider.GetRequiredService<IMarketSnapshotRepository>();
                var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();

                var assets = await assetRepository.GetAllAsync();

                if (assets is null || !assets.Any())
                {
                    _logger.LogInformation("No assets found to process.");
                    return;
                }

                var chunks = assets.Chunk(50);

                foreach (var chunk in chunks)
                {
                    try
                    {
                        stoppingToken.ThrowIfCancellationRequested();

                    var assetMap = chunk.ToDictionary(
                        x => x.PairSymbol.Trim().ToUpperInvariant(),
                        x => x);

                    var results = await binance.GetMarketSnapshotsAsync(assetMap.Keys);

                    foreach (var result in results)
                    {
                        if (!assetMap.TryGetValue(result.Symbol, out var asset))
                        {
                            continue;
                        }

                            var existingSnapshot = await marketSnapshotRepository.GetByAssetIdAsync(asset.Id);

                            if (existingSnapshot is null)
                            {
                                await marketSnapshotRepository.AddAsync(new MarketSnapshot
                                {
                                    AssetId = asset.Id,
                                    Price = result.Price,
                                    PriceChange24h = result.PriceChange,
                                    PriceChangePercentage24h = result.PriceChangePercent,
                                    Volume24h = result.Volume,
                                    UpdatedAt = DateTime.UtcNow
                                });
                            }
                            else
                            {
                                existingSnapshot.Price = result.Price;
                                existingSnapshot.PriceChange24h = result.PriceChange;
                                existingSnapshot.PriceChangePercentage24h = result.PriceChangePercent;
                                existingSnapshot.Volume24h = result.Volume;
                                existingSnapshot.UpdatedAt = DateTime.UtcNow;
                            }
                        }

                        await marketSnapshotRepository.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed chunk: {Symbols}", string.Join(", ", chunk.Select(x => x.PairSymbol)));
                    }
                }

                _logger.LogInformation("MarketSnapshotJob finished successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MarketSnapshotJob cancellation requested.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarketSnapshotJob failed.");
            }
        }
    }
}