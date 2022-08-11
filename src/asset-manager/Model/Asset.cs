namespace AssetManager.Model;

public class Asset : EntityBase
{
    public DateTime PurchaseDate { get; set; }
    public double PurchasePrice { get; set; }
    public string AssetTag { get; set; }
    public string Description { get; set; }

    public Guid AssetTypeId { get; set; }
    public AssetType AssetType { get; set; }

    public Guid LocationId { get; set; }
    public Location Location { get; set; }
}