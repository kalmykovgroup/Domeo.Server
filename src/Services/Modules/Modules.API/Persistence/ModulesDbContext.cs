using Domeo.Shared.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Modules.API.Entities;

namespace Modules.API.Persistence;

public sealed class ModulesDbContext : DbContext
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    public ModulesDbContext(DbContextOptions<ModulesDbContext> options) : base(options)
    {
    }

    public ModulesDbContext(
        DbContextOptions<ModulesDbContext> options,
        AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

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
        modelBuilder.HasDefaultSchema("modules");

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
