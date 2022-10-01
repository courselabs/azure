using AssetManager.Model;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace AssetManager.Services;

public class MongoAssetService : IAssetService
{
    private static bool _SeedDataChecked;
    private readonly IConfiguration _config;
    private readonly IMongoCollection<AssetType> _assetTypes;
    private readonly IMongoCollection<Location> _locations;
    private readonly IMongoCollection<Asset> _assets;


    public MongoAssetService(IMongoDatabase database, IConfiguration config)
    {
        _assetTypes = database.GetCollection<AssetType>("AssetTypes");
        _locations = database.GetCollection<Location>("Locations");
        _assets = database.GetCollection<Asset>("Assets");
        _config = config;
        EnsureSeedData();
    }

    public async Task<IEnumerable<AssetType>> GetAssetTypesAsync()
    {
        return await _assetTypes.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Location>> GetLocationsAsync()
    {
        return await _locations.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetAssetsAsync()
    {
        return await _assets.Find(_ => true).ToListAsync();
    }

    private void EnsureSeedData()
    {
        if (!_SeedDataChecked)
        {
            if (_locations.CountDocuments(_ => true) == 0)
            {
                foreach (var location in SeedData.Locations.GetSeedData())
                {
                    _locations.InsertOne(location);
                }
            }
            if (_assetTypes.CountDocuments(_ => true) == 0)
            {
                foreach (var assetType in SeedData.AssetTypes.GetSeedData())
                {
                    _assetTypes.InsertOne(assetType);
                }
            }
            _SeedDataChecked = true;
        }
    }
}