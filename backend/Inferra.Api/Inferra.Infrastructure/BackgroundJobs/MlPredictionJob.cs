using Inferra.Application.Interfaces.Integrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.BackgroundJobs
{
    public class MlPredictionJob: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MlPredictionJob> _logger;

        public MlPredictionJob(
            IServiceScopeFactory scopeFactory,
            ILogger<MlPredictionJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var mlClient = scope.ServiceProvider.GetRequiredService<IMlClient>();

                    _logger.LogInformation("Starting daily predictions...");

                    await mlClient.TriggerDailyPredictionAsync(stoppingToken);

                    _logger.LogInformation("Daily predictions triggered.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Daily prediction failed.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
