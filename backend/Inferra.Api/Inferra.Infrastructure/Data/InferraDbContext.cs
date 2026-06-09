using Inferra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inferra.Infrastructure.Data
{

    public class InferraDbContext:DbContext
    {
        public InferraDbContext(DbContextOptions<InferraDbContext> options) : base(options)
        {
        }

        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<DailyCandle> DailyCandles => Set<DailyCandle>();
        public DbSet<MarketSnapshot> MarketSnapshots => Set<MarketSnapshot>();
        public DbSet<ForecastRun> ForecastRuns => Set<ForecastRun>();
        public DbSet<ForecastPoint> ForecastPoints => Set<ForecastPoint>();
        public DbSet<SentimentSnapshot> SentimentSnapshots => Set<SentimentSnapshot>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
                .HasQueryFilter(a => !a.IsDeleted);

            modelBuilder.Entity<Asset>()
                .HasIndex(x => x.Symbol)
                .IsUnique();

            modelBuilder.Entity<Asset>()
                .HasIndex(x => x.PairSymbol)
                .IsUnique();

            modelBuilder.Entity<DailyCandle>()
                .HasIndex(x => new { x.AssetId, x.Date })
                .IsUnique();

            modelBuilder.Entity<MarketSnapshot>()
                .HasIndex(x => x.AssetId)
                .IsUnique();

            modelBuilder.Entity<MarketSnapshot>()
                .HasOne(x => x.Asset)
                .WithOne(x => x.MarketSnapshot)
                .HasForeignKey<MarketSnapshot>(x => x.AssetId);

            modelBuilder.Entity<SentimentSnapshot>()
                .HasIndex(x => x.AssetId)
                .IsUnique();

            modelBuilder.Entity<SentimentSnapshot>()
                .HasOne(x => x.Asset)
                .WithOne(x => x.SentimentSnapshot)
                .HasForeignKey<SentimentSnapshot>(x => x.AssetId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
