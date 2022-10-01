using AssetManager.Model;

namespace AssetManager.SeedData;

public static class Locations 
{
    public static IEnumerable<Location> GetSeedData()
    {
        return new Location[] {
                new Location
                {
                    Country = "USA",
                    PostalCode = "DC 20500",
                    AddressLine1 = "1600 Pennsylvania Ave NW"
                },
                new Location
                {
                    Country = "UK",
                    PostalCode = "SW1A 0AA",
                    AddressLine1 = "Houses of Parliament"
                }
        };   
    }
}