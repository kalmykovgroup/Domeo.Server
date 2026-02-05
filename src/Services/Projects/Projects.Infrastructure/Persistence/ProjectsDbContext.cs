using Domeo.Shared.Application;
using Domeo.Shared.Domain;
using Domeo.Shared.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Projects.Domain.Entities;

namespace Projects.Infrastructure.Persistence;

public sealed class ProjectsDbContext : DbContext, IUnitOfWork
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    public ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : base(options)
    {
    }

    public ProjectsDbContext(
        DbContextOptions<ProjectsDbContext> options,
        AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomVertex> RoomVertices => Set<RoomVertex>();
    public DbSet<RoomEdge> RoomEdges => Set<RoomEdge>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Cabinet> Cabinets => Set<Cabinet>();
    public DbSet<CabinetMaterial> CabinetMaterials => Set<CabinetMaterial>();
    public DbSet<CabinetHardwareOverride> CabinetHardwareOverrides => Set<CabinetHardwareOverride>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditInterceptor is not null)
            optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("projects");

        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("projects");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(x => x.ClientId).HasColumnName("client_id").IsRequired();
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.Notes).HasColumnName("notes");
            builder.Property(x => x.QuestionnaireData).HasColumnName("questionnaire_data").HasColumnType("jsonb");
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.ClientId);
            builder.HasIndex(x => x.UserId);
            builder.HasQueryFilter(x => x.DeletedAt == null);
        });

        modelBuilder.Entity<Room>(builder =>
        {
            builder.ToTable("rooms");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ProjectId).HasColumnName("project_id").IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            builder.Property(x => x.CeilingHeight).HasColumnName("ceiling_height").IsRequired().HasDefaultValue(2700);
            builder.Property(x => x.OrderIndex).HasColumnName("order_index").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.ProjectId);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomVertex>(builder =>
        {
            builder.ToTable("room_vertices");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.RoomId).HasColumnName("room_id").IsRequired();
            builder.Property(x => x.X).HasColumnName("x").IsRequired();
            builder.Property(x => x.Y).HasColumnName("y").IsRequired();
            builder.Property(x => x.OrderIndex).HasColumnName("order_index").IsRequired();

            builder.HasIndex(x => x.RoomId);
            builder.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoomEdge>(builder =>
        {
            builder.ToTable("room_edges");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.RoomId).HasColumnName("room_id").IsRequired();
            builder.Property(x => x.StartVertexId).HasColumnName("start_vertex_id").IsRequired();
            builder.Property(x => x.EndVertexId).HasColumnName("end_vertex_id").IsRequired();
            builder.Property(x => x.WallHeight).HasColumnName("wall_height").IsRequired().HasDefaultValue(2700);
            builder.Property(x => x.HasWindow).HasColumnName("has_window").IsRequired();
            builder.Property(x => x.HasDoor).HasColumnName("has_door").IsRequired();
            builder.Property(x => x.OrderIndex).HasColumnName("order_index").IsRequired();

            builder.HasIndex(x => x.RoomId);
            builder.HasIndex(x => x.StartVertexId);
            builder.HasIndex(x => x.EndVertexId);
            builder.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<RoomVertex>().WithMany().HasForeignKey(x => x.StartVertexId).OnDelete(DeleteBehavior.NoAction);
            builder.HasOne<RoomVertex>().WithMany().HasForeignKey(x => x.EndVertexId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Zone>(builder =>
        {
            builder.ToTable("zones");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.EdgeId).HasColumnName("edge_id").IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);
            builder.Property(x => x.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(50).IsRequired();
            builder.Property(x => x.StartX).HasColumnName("start_x").IsRequired();
            builder.Property(x => x.EndX).HasColumnName("end_x").IsRequired();

            builder.HasIndex(x => x.EdgeId);
            builder.HasOne<RoomEdge>().WithMany().HasForeignKey(x => x.EdgeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cabinet>(builder =>
        {
            builder.ToTable("cabinets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.RoomId).HasColumnName("room_id").IsRequired();
            builder.Property(x => x.EdgeId).HasColumnName("edge_id");
            builder.Property(x => x.ZoneId).HasColumnName("zone_id");
            builder.Property(x => x.AssemblyId).HasColumnName("assembly_id");
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
            builder.HasIndex(x => x.AssemblyId);
            builder.HasOne<Room>().WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<RoomEdge>().WithMany().HasForeignKey(x => x.EdgeId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne<Zone>().WithMany().HasForeignKey(x => x.ZoneId).OnDelete(DeleteBehavior.SetNull);
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
            builder.HasOne<Cabinet>().WithMany().HasForeignKey(x => x.CabinetId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CabinetHardwareOverride>(builder =>
        {
            builder.ToTable("cabinet_hardware_overrides");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.CabinetId).HasColumnName("cabinet_id").IsRequired();
            builder.Property(x => x.AssemblyPartId).HasColumnName("assembly_part_id").IsRequired();
            builder.Property(x => x.ComponentId).HasColumnName("component_id");
            builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(50);
            builder.Property(x => x.QuantityFormula).HasColumnName("quantity_formula").HasMaxLength(255);
            builder.Property(x => x.PositionXFormula).HasColumnName("position_x_formula").HasMaxLength(255);
            builder.Property(x => x.PositionYFormula).HasColumnName("position_y_formula").HasMaxLength(255);
            builder.Property(x => x.PositionZFormula).HasColumnName("position_z_formula").HasMaxLength(255);
            builder.Property(x => x.IsEnabled).HasColumnName("is_enabled").IsRequired().HasDefaultValue(true);
            builder.Property(x => x.MaterialId).HasColumnName("material_id").HasMaxLength(100);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.CabinetId);
            builder.HasIndex(x => x.AssemblyPartId);
            builder.HasIndex(x => new { x.CabinetId, x.AssemblyPartId }).IsUnique();
            builder.HasOne<Cabinet>().WithMany().HasForeignKey(x => x.CabinetId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
