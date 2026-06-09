using Inferra.Application.Interfaces;
using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Interfaces.Repositories;
using Inferra.Infrastructure.BackgroundJobs;
using Inferra.Infrastructure.Data;
using Inferra.Infrastructure.Data.Repositories;
using Inferra.Infrastructure.Data.Seed;
using Inferra.Infrastructure.Integrations.Binance;
using Inferra.Infrastructure.Integrations.MlClient.Inferra.Infrastructure.Integrations.Ml;
using Inferra.Infrastructure.Integrations.NewsApi;
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
            services.AddScoped<IForecastRunRepository, ForecastRunRepository>();
            services.AddScoped<ISentimentSnapshotRepository, SentimentSnapshotRepository>();

            services.AddHostedService<DailyCandleUpdateJob>();
            services.AddHostedService<MarketSnapshotJob>();
            


            services.AddHttpClient<IMlClient, MlClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["MlService:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("X-Api-Key", configuration["MlService:ApiKey"]);
            });

            services.AddHostedService<MlTrainingJob>();
            services.AddHostedService<MlPredictionJob>();

            var newsApiBaseUrl = configuration["Integrations:NewsApi:BaseUrl"]
                ?? throw new InvalidOperationException("Missing configuration: Integrations:NewsApi:BaseUrl");

            var newsApiKey = configuration["Integrations:NewsApi:ApiKey"]
                ?? throw new InvalidOperationException("Missing configuration: Integrations:NewsApi:ApiKey");

            services.AddHttpClient("NewsApi", client =>
            {
                client.BaseAddress = new Uri(newsApiBaseUrl);
                client.DefaultRequestHeaders.Add("User-Agent", "Inferra/1.0");
            });

            services.AddScoped<INewsApiClient>(sp =>
            {
                var factory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = factory.CreateClient("NewsApi");
                return new InferraNewsApiClient(httpClient, newsApiKey);
            });

            services.AddHostedService<NewsSentimentJob>();
            return services;

        }
    }
}
