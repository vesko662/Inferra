using Inferra.Api.Models;
using Inferra.Application.Interfaces.Services;
using Inferra.Application.Models.Assets;
using Microsoft.AspNetCore.Mvc;

namespace Inferra.Api.Controllers
{
    [ApiController]
    [Route("api/assets")]
    public class AssetsController : ControllerBase
    {

        private readonly IAssetQueryService _assetQueryService;

        public AssetsController(IAssetQueryService assetQueryService)
        {
            _assetQueryService = assetQueryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<AssetListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var assets = await _assetQueryService.GetAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("{symbol}")]
        [ProducesResponseType(typeof(AssetDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySymbol(string symbol)
        {
            var asset = await _assetQueryService.GetAssetBySymbolAsync(symbol);
            if (asset is null)
            {
                return NotFound(new { message = $"Asset with symbol '{symbol}' was not found." });
            }

            return Ok(asset);
        }
    }
}
