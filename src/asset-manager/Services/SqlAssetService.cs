using AssetManager.Model;
using AssetManager.Sql;
using Microsoft.EntityFrameworkCore;

namespace AssetManager.Services;

public class SqlAssetService : IAssetService
{
    private static bool _SeedDataChecked;
    private readonly AssetContext _context;
    private readonly IConfiguration _config;

    public SqlAssetService(AssetContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        EnsureSeedData();
    }

    public async Task<IEnumerable<AssetType>> GetAssetTypesAsync()
    {
        return await _context.AssetTypes.ToArrayAsync();
    }

    public async Task<IEnumerable<Location>> GetLocationsAsync()
    {
        return await _context.Locations.ToArrayAsync();
    }

    public async Task<IEnumerable<Asset>> GetAssetsAsync()
    {
        return await _context.Assets.ToArrayAsync();
    }

    private void EnsureSeedData()
    {
        if (!_SeedDataChecked)
        {
            if (_context.Locations.Count() == 0)
            {
                foreach (var location in SeedData.Locations.GetSeedData())
                {
                    _context.Locations.Add(location);
                }
            }

            if (_context.AssetTypes.Count() == 0)
            {
                foreach (var assetType in SeedData.AssetTypes.GetSeedData())
                {
                    _context.AssetTypes.Add(assetType);
                }
            }
            _context.SaveChanges();
            _SeedDataChecked = true;
        }
    }
}