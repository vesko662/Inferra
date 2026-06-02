using Inferra.Api.Models;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Assets;
using Inferra.Application.Models.Forecasts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [Route("api/assets/{symbol}/forecast")]
    [ApiController]
    public class AssetForecastController : ControllerBase
    {
        private readonly IForecastQueryService _forecastQueryService;

        public AssetForecastController(IForecastQueryService forecastQueryService)
        {
            _forecastQueryService = forecastQueryService;
        }

        [HttpGet("latest")]
        [ProducesResponseType(typeof(AssetLatestForecastDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLatest(string symbol)
        {
            var result = await _forecastQueryService.GetLatestBySymbolAsync(symbol);

            if (result is null)
            {
                return NotFound(new { message = $"Asset with symbol '{symbol}' was not found." });
            }

            if (result.Forecasts.Count == 0)
            {
                return NotFound(new { message = $"No saved forecast was found for asset '{symbol}'." });
            }

            return Ok(result);
        }
    }
}
