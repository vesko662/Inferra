using Inferra.Application.Interfaces.Integrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.BackgroundJobs 
{
    public class MlTrainingJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MlTrainingJob> _logger;

        public MlTrainingJob(
            IServiceScopeFactory scopeFactory,
            ILogger<MlTrainingJob> logger)
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

                    _logger.LogInformation("Starting ML training...");

                    await mlClient.TriggerTrainingAsync(stoppingToken);

                    _logger.LogInformation("ML training triggered.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ML training failed.");
                }

                await Task.Delay(TimeSpan.FromDays(7), stoppingToken);
            }
        }
    }
}
