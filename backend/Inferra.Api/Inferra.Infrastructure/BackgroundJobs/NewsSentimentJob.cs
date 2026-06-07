using Inferra.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inferra.Infrastructure.BackgroundJobs
{
    public class NewsSentimentJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NewsSentimentJob> _logger;

        public NewsSentimentJob(IServiceScopeFactory scopeFactory, ILogger<NewsSentimentJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NewsSentimentJob стартира.");

            await RunOnceAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(now.Hour >= 6 ? 1 : 0).AddHours(6);
                var delay = nextRun - now;

                _logger.LogInformation("Следващото извличане на новини е след {Hours}ч {Minutes}м", delay.Hours, delay.Minutes);

                await Task.Delay(delay, stoppingToken);

                await RunOnceAsync(stoppingToken);
            }
        }

        private async Task RunOnceAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<INewsSentimentService>();

                _logger.LogInformation("NewsSentimentJob: извличане на новини...");
                await service.GetSentimentAsync();
                _logger.LogInformation("NewsSentimentJob: завърши успешно.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NewsSentimentJob: грешка при извличане на новини.");
            }
        }
    }
}
