using Inferra.Application.Interfaces;
using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Interfaces.Repositories;
using Inferra.Infrastructure.BackgroundJobs;
using Inferra.Infrastructure.Data;
using Inferra.Infrastructure.Data.Repositories;
using Inferra.Infrastructure.Data.Seed;
using Inferra.Infrastructure.Integrations.Binance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Inferra.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<InferraDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient<IBinanceClient, BinanceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Integrations:BinanceApi:BaseUrl"]);
            });

            var coinGeckoSection = configuration.GetSection("Integrations:CoinGeckoApi");

            var coinGeckoBaseUrl = coinGeckoSection.GetValue<string>("BaseUrl")
                ?? throw new InvalidOperationException("Missing configuration: Integrations:CoinGeckoApi:BaseUrl");

            var coinGeckoApiKey = coinGeckoSection.GetValue<string>("ApiKey")
                ?? throw new InvalidOperationException("Missing configuration: Integrations:CoinGeckoApi:ApiKey");

            services.AddHttpClient<ICoingeckoClient, CoingeckoClient>(client =>
            {
                client.BaseAddress = new Uri(coinGeckoBaseUrl);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("x-cg-demo-api-key", coinGeckoApiKey);
            });


            services.AddScoped<AssetsSeeder>();

            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IDailyCandleRepository, DailyCandleRepository>();
            services.AddScoped<IMarketSnapshotRepository, MarketSnapshotRepository>();

            services.AddHostedService<MarketSnapshotJob>();


            return services;

        }
    }
}
