
namespace Inferra.Application.Models.Forecasts
{
    public class ModelForecastInputDto
    {
        public string ModelType { get; init; } = null!;

        public string ModelVersion { get; init; } = null!;

        public DateOnly ForecastStartDate { get; init; }
        public DateOnly ForecastEndDate { get; init; }

        public int HorizonDays { get; init; }

        public List<ForecastPointInputDto> Points { get; init; } = new();
    }
}
