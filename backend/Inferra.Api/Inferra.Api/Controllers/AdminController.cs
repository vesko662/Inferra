using Inferra.Application.Interfaces.Integrations;
using Inferra.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAssetAdminService _assetAdminService;
        private readonly IMlClient _mlClient;
        private readonly INewsSentimentService _newsSentimentService;

        public AdminController(
            IAssetAdminService assetAdminService,
            IMlClient mlClient,
            INewsSentimentService newsSentimentService)
        {
            _assetAdminService = assetAdminService;
            _mlClient = mlClient;
            _newsSentimentService = newsSentimentService;
        }

       
        [HttpPatch("assets/{symbol}/ml-enabled")]
        public async Task<IActionResult> SetMlEnabled(string symbol, [FromQuery] bool enabled)
        {
            var found = await _assetAdminService.SetMlEnabledAsync(symbol, enabled);
            if (!found) return NotFound(new { message = $"Asset '{symbol}' not found." });
            return NoContent();
        }

        [HttpPatch("assets/{symbol}/news-enabled")]
        public async Task<IActionResult> SetNewsEnabled(string symbol, [FromQuery] bool enabled)
        {
            var found = await _assetAdminService.SetNewsEnabledAsync(symbol, enabled);
            if (!found) return NotFound(new { message = $"Asset '{symbol}' not found." });
            return NoContent();
        }

        [HttpPost("ml/train")]
        public async Task<IActionResult> TriggerTraining(CancellationToken cancellationToken)
        {
            await _mlClient.TriggerTrainingAsync(cancellationToken);
            return Accepted(new { message = "Training pipeline triggered." });
        }

        [HttpPost("ml/predict-daily")]
        public async Task<IActionResult> TriggerDailyPrediction(CancellationToken cancellationToken)
        {
            await _mlClient.TriggerDailyPredictionAsync(cancellationToken);
            return Accepted(new { message = "Daily prediction pipeline triggered." });
        }

        [HttpPost("ml/sentiment")]
        public async Task<IActionResult> TriggerSentiment()
        {
            await _newsSentimentService.GetSentimentAsync();
            return Accepted(new { message = "Sentiment pipeline triggered." });
        }

        [HttpDelete("assets/{symbol}")]
        public async Task<IActionResult> DeleteAsset(string symbol)
        {
            var found = await _assetAdminService.DeleteAsync(symbol);
            if (!found) return NotFound(new { message = $"Asset '{symbol}' not found." });
            return NoContent();
        }
    }
}
