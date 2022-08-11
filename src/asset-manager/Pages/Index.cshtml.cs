using AssetManager.Model;
using AssetManager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AssetManager.Pages;

public class IndexModel : PageModel
{
    private readonly IAssetService _assetService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<Asset> Assets { get; private set; }
    public IEnumerable<AssetType> AssetTypes { get; private set; }
    public IEnumerable<Location> Locations { get; private set; }

    public IndexModel(IAssetService assetService, ILogger<IndexModel> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
    {
        _logger.LogDebug("GET / called");

        AssetTypes = await _assetService.GetAssetTypesAsync();
        _logger.LogDebug($"Fetched {AssetTypes.Count()} asset types");

        Locations = await _assetService.GetLocationsAsync();
        _logger.LogDebug($"Fetched {Locations.Count()} locations");

        Assets = await _assetService.GetAssetsAsync();
        _logger.LogDebug($"Fetched {Assets.Count()} assets");

        return Page();
    }
}
