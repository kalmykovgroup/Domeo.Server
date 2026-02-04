using Domeo.Shared.Infrastructure.Audit;
using Materials.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Materials.API.Persistence;

public sealed class MaterialsDbContext : DbContext
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    public MaterialsDbContext(DbContextOptions<MaterialsDbContext> options) : base(options)
    {
    }

    public MaterialsDbContext(
        DbContextOptions<MaterialsDbContext> options,
        AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierMaterial> SupplierMaterials => Set<SupplierMaterial>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditInterceptor is not null)
            optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("materials");

        // MaterialCategory
        modelBuilder.Entity<MaterialCategory>(builder =>
        {
            builder.ToTable("material_categories");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ParentId).HasColumnName("parent_id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Level).HasColumnName("level").IsRequired();
            builder.Property(x => x.OrderIndex).HasColumnName("order_index").IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.ParentId);
        });

        // Material
        modelBuilder.Entity<Material>(builder =>
        {
            builder.ToTable("materials");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20).IsRequired().HasDefaultValue("sqm");
            builder.Property(x => x.Color).HasColumnName("color").HasMaxLength(50);
            builder.Property(x => x.TextureUrl).HasColumnName("texture_url").HasMaxLength(500);
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.CategoryId);
        });

        // Supplier
        modelBuilder.Entity<Supplier>(builder =>
        {
            builder.ToTable("suppliers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Company).HasColumnName("company").HasMaxLength(200).IsRequired();
            builder.Property(x => x.ContactFirstName).HasColumnName("contact_first_name").HasMaxLength(100);
            builder.Property(x => x.ContactLastName).HasColumnName("contact_last_name").HasMaxLength(100);
            builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
            builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.Company);
        });

        // SupplierMaterial
        modelBuilder.Entity<SupplierMaterial>(builder =>
        {
            builder.ToTable("supplier_materials");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.MaterialId).HasColumnName("material_id").IsRequired();
            builder.Property(x => x.SupplierId).HasColumnName("supplier_id").IsRequired();
            builder.Property(x => x.Price).HasColumnName("price").HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Currency).HasColumnName("currency").HasMaxLength(10).IsRequired().HasDefaultValue("RUB");
            builder.Property(x => x.MinOrderQty).HasColumnName("min_order_qty").IsRequired().HasDefaultValue(1);
            builder.Property(x => x.LeadTimeDays).HasColumnName("lead_time_days").IsRequired();
            builder.Property(x => x.InStock).HasColumnName("in_stock").IsRequired().HasDefaultValue(true);
            builder.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(100);
            builder.Property(x => x.Notes).HasColumnName("notes");
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

            builder.HasIndex(x => new { x.MaterialId, x.SupplierId }).IsUnique();
            builder.HasIndex(x => x.SupplierId);
        });
    }
}
