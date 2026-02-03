using Domeo.Shared.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Projects.API.Entities;

namespace Projects.API.Persistence;

public sealed class ProjectsDbContext : DbContext
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
