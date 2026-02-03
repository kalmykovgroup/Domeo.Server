using Catalog.API.Entities;
using Domeo.Shared.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Persistence;

public sealed class CatalogDbContext : DbContext
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options,
        AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<MaterialCategory> MaterialCategories => Set<MaterialCategory>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierMaterial> SupplierMaterials => Set<SupplierMaterial>();
    public DbSet<ModuleCategory> ModuleCategories => Set<ModuleCategory>();
    public DbSet<ModuleType> ModuleTypes => Set<ModuleType>();
    public DbSet<Hardware> Hardware => Set<Hardware>();
    public DbSet<ModuleHardware> ModuleHardware => Set<ModuleHardware>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditInterceptor is not null)
            optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

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

        // ModuleCategory
        modelBuilder.Entity<ModuleCategory>(builder =>
        {
            builder.ToTable("module_categories");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").HasMaxLength(100);
            builder.Property(x => x.ParentId).HasColumnName("parent_id").HasMaxLength(100);
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasColumnName("description");
            builder.Property(x => x.OrderIndex).HasColumnName("order_index").IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.ParentId);
        });

        // ModuleType
        modelBuilder.Entity<ModuleType>(builder =>
        {
            builder.ToTable("module_types");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.CategoryId).HasColumnName("category_id").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.WidthDefault).HasColumnName("width_default").IsRequired();
            builder.Property(x => x.WidthMin).HasColumnName("width_min").IsRequired();
            builder.Property(x => x.WidthMax).HasColumnName("width_max").IsRequired();
            builder.Property(x => x.HeightDefault).HasColumnName("height_default").IsRequired();
            builder.Property(x => x.HeightMin).HasColumnName("height_min").IsRequired();
            builder.Property(x => x.HeightMax).HasColumnName("height_max").IsRequired();
            builder.Property(x => x.DepthDefault).HasColumnName("depth_default").IsRequired();
            builder.Property(x => x.DepthMin).HasColumnName("depth_min").IsRequired();
            builder.Property(x => x.DepthMax).HasColumnName("depth_max").IsRequired();
            builder.Property(x => x.PanelThickness).HasColumnName("panel_thickness").IsRequired().HasDefaultValue(16);
            builder.Property(x => x.BackPanelThickness).HasColumnName("back_panel_thickness").IsRequired().HasDefaultValue(4);
            builder.Property(x => x.FacadeThickness).HasColumnName("facade_thickness").IsRequired().HasDefaultValue(18);
            builder.Property(x => x.FacadeGap).HasColumnName("facade_gap").IsRequired().HasDefaultValue(2);
            builder.Property(x => x.FacadeOffset).HasColumnName("facade_offset").IsRequired();
            builder.Property(x => x.ShelfSideGap).HasColumnName("shelf_side_gap").IsRequired().HasDefaultValue(2);
            builder.Property(x => x.ShelfRearInset).HasColumnName("shelf_rear_inset").IsRequired().HasDefaultValue(20);
            builder.Property(x => x.ShelfFrontInset).HasColumnName("shelf_front_inset").IsRequired().HasDefaultValue(10);
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.Type).IsUnique();
            builder.HasIndex(x => x.CategoryId);
        });

        // Hardware
        modelBuilder.Entity<Hardware>(builder =>
        {
            builder.ToTable("hardware");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100);
            builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(100);
            builder.Property(x => x.ModelUrl).HasColumnName("model_url").HasMaxLength(500);
            builder.Property(x => x.Params).HasColumnName("params").HasColumnType("jsonb");
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

            builder.HasIndex(x => x.Type);
        });

        // ModuleHardware
        modelBuilder.Entity<ModuleHardware>(builder =>
        {
            builder.ToTable("module_hardware");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.ModuleTypeId).HasColumnName("module_type_id").IsRequired();
            builder.Property(x => x.HardwareId).HasColumnName("hardware_id").IsRequired();
            builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            builder.Property(x => x.QuantityFormula).HasColumnName("quantity_formula").HasMaxLength(100).IsRequired().HasDefaultValue("1");
            builder.Property(x => x.PositionXFormula).HasColumnName("position_x_formula").HasMaxLength(200);
            builder.Property(x => x.PositionYFormula).HasColumnName("position_y_formula").HasMaxLength(200);
            builder.Property(x => x.PositionZFormula).HasColumnName("position_z_formula").HasMaxLength(200);

            builder.HasIndex(x => x.ModuleTypeId);
            builder.HasIndex(x => x.HardwareId);
        });
    }
}
