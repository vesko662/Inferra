using Inferra.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Inferra.Domain.Entities
{
    [Index(nameof(AssetId), nameof(ModelType), nameof(GeneratedAt))]
    [Index(nameof(AssetId), nameof(ModelType), nameof(IsLatest))]
    public class ForecastRun
    {
        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public ForecastModelType ModelType { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModelVersion { get; set; } = null!;

        public DateTime GeneratedAt { get; set; }

        public DateOnly ForecastStartDate { get; set; }
        public DateOnly ForecastEndDate { get; set; }

        public int HorizonDays { get; set; }

        public bool IsLatest { get; set; }

        public ICollection<ForecastPoint> Points { get; set; } = new List<ForecastPoint>();
    }
}
