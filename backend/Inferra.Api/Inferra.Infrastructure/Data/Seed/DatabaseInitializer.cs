using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.Data.Seed
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<InferraDbContext>();
            await context.Database.MigrateAsync();

            var tokenSeeder = scope.ServiceProvider.GetRequiredService<AssetsSeeder>();
            await tokenSeeder.SeedAsync();
        }
    }
}
