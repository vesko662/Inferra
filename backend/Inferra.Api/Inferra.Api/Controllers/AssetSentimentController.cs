using Inferra.Api.Models;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [Authorize]
    [Route("api/assets/{symbol}/sentiment")]
    [ApiController]
    public class AssetSentimentController : ControllerBase
    {
        private readonly IAssetQueryService _assetQueryService;

        public AssetSentimentController(IAssetQueryService assetQueryService)
        {
            _assetQueryService = assetQueryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SentimentSnapshotDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSentiment(string symbol)
        {
            var result = await _assetQueryService.GetSentimentBySymbolAsync(symbol);

            if (result is null)
            {
                return NotFound(new { message = $"No sentiment data found for asset '{symbol}'." });
            }

            return Ok(result);
        }
    }
}
