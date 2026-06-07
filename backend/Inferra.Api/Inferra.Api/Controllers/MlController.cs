using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Forecasts;
using Inferra.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [Route("api/ml")]
    [ApiController]
    public class MlController : ControllerBase
    {
        private readonly IForecastIngestionService _forecastIngestionService;
        private readonly IMlDatasetService _mlDatasetService;
        private readonly INewsSentimentService _newsSentimentService;

        public MlController(
            IForecastIngestionService forecastIngestionService,
            IMlDatasetService mlDatasetService,
            INewsSentimentService newsSentimentService)
        {
            _forecastIngestionService = forecastIngestionService;
            _mlDatasetService = mlDatasetService;
            _newsSentimentService = newsSentimentService;
        }
        [HttpGet("datasets/training")]
        public async Task<IActionResult> GetTrainingDataset()
        {
            var result = await _mlDatasetService.GetTrainingDatasetAsync();
            return Ok(result);
        }

        [HttpGet("datasets/forecast")]
        public async Task<IActionResult> GetForecastDataset()
        {
            var result = await _mlDatasetService.GetForecastDatasetAsync();
            return Ok(result);
        }

        [HttpPost("daily")]
        public async Task<IActionResult> SaveDaily([FromBody] SaveDailyForecastRequest request)
        {
            var result = await _forecastIngestionService.SaveDailyForecastAsync(request);

            if (result.AssetNotFound)
            {
                return NotFound(new { message = $"Asset with symbol '{request.Symbol}' was not found." });
            }

            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Daily forecasts saved successfully." });
        }

        [HttpGet("sentiment")]
        public async Task<IActionResult> GetSentiment()
        {
            var result = await _newsSentimentService.GetSentimentAsync();
            return Ok(result);
        }
    }
}
