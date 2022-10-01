using AssetManager.Model;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AssetManager.Services;

public class BlobStorageAssetService : IAssetService
{
    private static bool _SeedDataChecked;
    private readonly IConfiguration _config;
    private readonly BlobContainerClient _containerClient;


    public BlobStorageAssetService(BlobContainerClient containerClient, IConfiguration config)
    {
        _containerClient = containerClient;
        _config = config;
        EnsureSeedData();
    }

    public async Task<IEnumerable<AssetType>> GetAssetTypesAsync()
    {
        return await DownloadAll<AssetType>();
    }

    public async Task<IEnumerable<Location>> GetLocationsAsync()
    {
        return await DownloadAll<Location>();
    }
    
    public async Task<IEnumerable<Asset>> GetAssetsAsync()
    {
        return await DownloadAll<Asset>();
    }

    private void EnsureSeedData()
    {
        if (!_SeedDataChecked)
        {
            var locations = _containerClient.GetBlobsByHierarchy(prefix: GetPrefix<Location>());
            if (locations.Count() == 0)
            {
                foreach (var location in SeedData.Locations.GetSeedData())
                {                
                    Upload(location);
                }
            }

            var assetTypes = _containerClient.GetBlobsByHierarchy(prefix: GetPrefix<AssetType>());
            if (assetTypes.Count() == 0)
            {
                foreach (var assetType in SeedData.AssetTypes.GetSeedData())
                {                
                    Upload(assetType);
                }
            }            
            _SeedDataChecked = true;
        }
    }

    private async Task<IEnumerable<TEntity>> DownloadAll<TEntity>() where TEntity : EntityBase
    {
        var items = new List<TEntity>();
        var blobs = _containerClient.GetBlobsByHierarchyAsync(prefix: GetPrefix<TEntity>());
        await foreach (BlobHierarchyItem blob in blobs)
        {
            if (blob.IsBlob)
            {
                var blobClient = _containerClient.GetBlobClient(blob.Blob.Name);
                var response = await blobClient.DownloadContentAsync();
                var item = response.Value.Content.ToObjectFromJson<TEntity>();                
                items.Add(item);
            }
        }
        return items;
    }


    private void Upload<TEntity>(TEntity entity) where TEntity : EntityBase
    {
        entity.Id = Guid.NewGuid();
        var path = $"{GetPrefix<TEntity>()}/{entity.Id}";
        var blobClient = _containerClient.GetBlobClient(path);
        blobClient.Upload(BinaryData.FromObjectAsJson(entity));
    }

    private string GetPrefix<TEntity>() where TEntity : EntityBase
    {
        return typeof(TEntity).ToString();
    }
}