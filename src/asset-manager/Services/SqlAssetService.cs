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
                _context.Locations.Add(new Location
                {
                    Country = "USA",
                    PostalCode = "DC 20500",
                    AddressLine1 = "1600 Pennsylvania Ave NW"
                });
                _context.Locations.Add(new Location
                {
                    Country = "UK",
                    PostalCode = "SW1A 0AA",
                    AddressLine1 = "Houses of Parliament"
                });
            }

            if (_context.AssetTypes.Count() == 0)
            {
                _context.AssetTypes.Add(new AssetType
                {
                    Description = "Laptop"
                });
                _context.AssetTypes.Add(new AssetType
                {
                    Description = "Desktop"
                });
                _context.AssetTypes.Add(new AssetType
                {
                    Description = "Phone"
                });
            }

            _context.SaveChanges();
            _SeedDataChecked = true;
        }
    }
}