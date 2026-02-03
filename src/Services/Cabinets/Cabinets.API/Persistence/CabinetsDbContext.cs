using Cabinets.API.Entities;
using Domeo.Shared.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;

namespace Cabinets.API.Persistence;

public sealed class CabinetsDbContext : DbContext
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    public CabinetsDbContext(DbContextOptions<CabinetsDbContext> options) : base(options)
    {
    }

    public CabinetsDbContext(
        DbContextOptions<CabinetsDbContext> options,
        AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Cabinet> Cabinets => Set<Cabinet>();
    public DbSet<CabinetMaterial> CabinetMaterials => Set<CabinetMaterial>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditInterceptor is not null)
            optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cabinets");

        modelBuilder.Entity<Cabinet>(builder =>
        {
            builder.ToTable("cabinets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.RoomId).HasColumnName("room_id").IsRequired();
            builder.Property(x => x.EdgeId).HasColumnName("edge_id");
            builder.Property(x => x.ZoneId).HasColumnName("zone_id");
            builder.Property(x => x.ModuleTypeId).HasColumnName("module_type_id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
            builder.Property(x => x.PlacementType).HasColumnName("placement_type").HasMaxLength(50).IsRequired();
            builder.Property(x => x.FacadeType).HasColumnName("facade_type").HasMaxLength(50);
            builder.Property(x => x.PositionX).HasColumnName("position_x").IsRequired();
            builder.Property(x => x.PositionY).HasColumnName("position_y").IsRequired();
            builder.Property(x => x.Rotation).HasColumnName("rotation").IsRequired();
            builder.Property(x => x.Width).HasColumnName("width").IsRequired();
            builder.Property(x => x.Height).HasColumnName("height").IsRequired();
            builder.Property(x => x.Depth).HasColumnName("depth").IsRequired();
            builder.Property(x => x.CalculatedPrice).HasColumnName("calculated_price").HasPrecision(18, 2);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.RoomId);
            builder.HasIndex(x => x.EdgeId);
            builder.HasIndex(x => x.ZoneId);
        });

        modelBuilder.Entity<CabinetMaterial>(builder =>
        {
            builder.ToTable("cabinet_materials");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
            builder.Property(x => x.MaterialType).HasColumnName("material_type").HasMaxLength(50).IsRequired();
            builder.Property(x => x.MaterialId).HasColumnName("material_id").IsRequired();

            builder.HasIndex(x => x.CabinetId);
            builder.HasIndex(x => new { x.CabinetId, x.MaterialType }).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domeo.Shared.Kernel.Domain.Abstractions.IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(Domeo.Shared.Kernel.Domain.Abstractions.IAuditableEntity.CreatedAt)).CurrentValue = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(Domeo.Shared.Kernel.Domain.Abstractions.IAuditableEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
