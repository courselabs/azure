using AssetManager.Model;
using Microsoft.EntityFrameworkCore;

namespace AssetManager.Sql;

public class AssetContext : DbContext
{
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetType> AssetTypes { get; set; }
    public DbSet<Location> Locations { get; set; }

    public AssetContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>().HasOne(x => x.AssetType);
        modelBuilder.Entity<Asset>().HasOne(x => x.Location);
    }
}