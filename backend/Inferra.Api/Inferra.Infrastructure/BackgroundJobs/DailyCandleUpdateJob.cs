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
    public class DailyCandleUpdateJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyCandleUpdateJob> _logger;

        public DailyCandleUpdateJob(IServiceScopeFactory scopeFactory, ILogger<DailyCandleUpdateJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Candle Job стартира.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddSeconds(15); 
                var delay = nextRun - now;

                _logger.LogInformation($"Следващото затваряне на свещта е след {delay.Hours}ч {delay.Minutes}м");

                await Task.Delay(delay, stoppingToken);

                try
                {
                    await UpdateDailyCandlesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Грешка при обновяване на дневната свещ.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }

        private async Task UpdateDailyCandlesAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var binanceClient = scope.ServiceProvider.GetRequiredService<IBinanceClient>();
            var candleRepository = scope.ServiceProvider.GetRequiredService<IDailyCandleRepository>();
            var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetRepository>();

            var assets = await assetRepository.GetAllAsync();

            foreach (var asset in assets)
            {
                var lastSavedDate = await candleRepository.GetLastDateByAssetIdAsync(asset.Id);
                var startTimeUtc = BuildStartTimeUtc(lastSavedDate);

                while (true)
                {
                    var batch = await binanceClient.GetDailyCandlesBatchAsync(
                        pairSymbol: asset.PairSymbol,
                        startTimeUtc: startTimeUtc,
                        endTimeUtc: null,
                        limit: 1000);

                    if (batch.Count == 0)
                    {
                        break;
                    }

                    var nowUtc = DateTimeOffset.UtcNow;
                    var candlesToInsert = new List<DailyCandle>(batch.Count);

                    foreach (var candle in batch.OrderBy(x => x.OpenTimeUtc))
                    {
                        if (candle.CloseTimeUtc > nowUtc)
                        {
                            continue;
                        }

                        if (lastSavedDate.HasValue && candle.Date <= lastSavedDate.Value)
                        {
                            continue;
                        }

                        candlesToInsert.Add(new DailyCandle
                        {
                            AssetId = asset.Id,
                            Date = candle.Date,
                            Open = candle.Open,
                            High = candle.High,
                            Low = candle.Low,
                            Close = candle.Close,
                            Volume = candle.Volume
                        });
                    }

                    if (candlesToInsert.Count > 0)
                    {
                        await candleRepository.AddRangeAsync(candlesToInsert);
                        await candleRepository.SaveChangesAsync();
                        lastSavedDate = candlesToInsert.Max(x => x.Date);
                    }

                    if (batch.Count < 1000)
                    {
                        break;
                    }

                    startTimeUtc = batch.Max(x => x.OpenTimeUtc).AddDays(1);

                }
            }
        }

        private  DateTimeOffset BuildStartTimeUtc(DateOnly? lastSavedDate)
        {
            if (!lastSavedDate.HasValue)
            {
                return DateTimeOffset.UnixEpoch;
            }

            var nextDateUtc = DateTime.SpecifyKind(
                lastSavedDate.Value.AddDays(1).ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc);

            return new DateTimeOffset(nextDateUtc);
        }
    }
}
