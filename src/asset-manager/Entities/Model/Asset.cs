namespace AssetManager.Entities;

public class Asset
{
    public Guid Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public double PurchasePrice { get; set; }
    public string AssetTag { get; set; }
    public string Description { get; set; }

    public Guid AssetTypeId { get; set; }
    public AssetType AssetType { get; set; }

    public Guid LocationId { get; set; }
    public Location Location { get; set; }
}