using Microsoft.EntityFrameworkCore;
using MockSupplier.API.Infrastructure.Persistence.Entities;

namespace MockSupplier.API.Infrastructure.Persistence;

public class SupplierDbContext(DbContextOptions<SupplierDbContext> options) : DbContext(options)
{
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<MaterialEntity> Materials => Set<MaterialEntity>();
    public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();
    public DbSet<OfferEntity> Offers => Set<OfferEntity>();
    public DbSet<BrandEntity> Brands => Set<BrandEntity>();
    public DbSet<CategoryAttributeEntity> CategoryAttributes => Set<CategoryAttributeEntity>();
    public DbSet<MaterialAttributeValueEntity> MaterialAttributeValues => Set<MaterialAttributeValueEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategoryEntity>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ParentId);
        });

        modelBuilder.Entity<MaterialEntity>(e =>
        {
            e.ToTable("materials");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CategoryId);
            e.HasIndex(x => x.BrandId);
        });

        modelBuilder.Entity<SupplierEntity>(e =>
        {
            e.ToTable("suppliers");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<OfferEntity>(e =>
        {
            e.ToTable("offers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MaterialId);
            e.HasIndex(x => x.SupplierId);
        });

        modelBuilder.Entity<BrandEntity>(e =>
        {
            e.ToTable("brands");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<CategoryAttributeEntity>(e =>
        {
            e.ToTable("category_attributes");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.CategoryId);
        });

        modelBuilder.Entity<MaterialAttributeValueEntity>(e =>
        {
            e.ToTable("material_attribute_values");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.MaterialId);
            e.HasIndex(x => x.AttributeId);
        });
    }
}
