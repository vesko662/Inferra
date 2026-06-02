using Inferra.Application.Interfaces.Services;
using Inferra.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Application.DependencyInjection
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAssetQueryService, AssetQueryService>();
            services.AddScoped<IForecastQueryService, ForecastQueryService>();
            services.AddScoped<IForecastIngestionService, ForecastIngestionService>();
            services.AddScoped<IMlDatasetService, MlDatasetService>();
            return services;
        }
    }
}
