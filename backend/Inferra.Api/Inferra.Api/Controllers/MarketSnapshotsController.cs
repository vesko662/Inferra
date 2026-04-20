using Inferra.Api.Models;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Assets;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [ApiController]
    [Route("api/assets/{symbol}/snapshots")]
    public class MarketSnapshotsController : ControllerBase
    {
        private readonly IAssetQueryService _assetQueryService;

        public MarketSnapshotsController(IAssetQueryService assetQueryService)
        {
            _assetQueryService = assetQueryService;
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(MarketSnapshotDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLatest(string symbol)
        {
            var snapshot = await _assetQueryService.GetLatestSnapshotBySymbolAsync(symbol);
            if (snapshot is null)
            {
                return NotFound(new { message = $"Snapshot for asset '{symbol}' was not found." });
            }

            return Ok(snapshot);
        }
    }
}
