using AssetManager.Model;

namespace AssetManager.Services;

public interface IAssetService
{
    Task<IEnumerable<AssetType>> GetAssetTypesAsync();
    
    Task<IEnumerable<Location>> GetLocationsAsync();
    
    Task<IEnumerable<Asset>> GetAssetsAsync();
}