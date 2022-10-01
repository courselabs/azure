using AssetManager.Model;

namespace AssetManager.SeedData;

public static class AssetTypes
{
    public static IEnumerable<AssetType> GetSeedData()
    {
        return new AssetType[] {
                new AssetType
                {
                    Description = "Laptop"
                },
                new AssetType
                {
                    Description = "Desktop"
                },
                new AssetType
                {
                    Description = "Phone"
                }
        };
    }
}