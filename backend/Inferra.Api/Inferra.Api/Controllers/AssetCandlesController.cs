using Inferra.Api.Models;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Assets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [ApiController]
    [Route("api/assets/{symbol}/candles")]
    public class AssetCandlesController : ControllerBase
    {
        private readonly IAssetQueryService _assetQueryService;

        public AssetCandlesController(IAssetQueryService assetQueryService)
        {
            _assetQueryService = assetQueryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<DailyCandleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string symbol, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        {
            if (from.HasValue && to.HasValue && from > to)
            {
                return BadRequest(new { message = "'from' must be less than or equal to 'to'." });
            }

            var candles = await _assetQueryService.GetCandlesBySymbolAsync(symbol, from, to);
            if (candles is null)
            {
                return NotFound(new { message = $"Asset with symbol '{symbol}' was not found." });
            }

            return Ok(candles);
        }
    }
}
