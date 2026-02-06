using System.Text.Json;
using System.Text.Json.Serialization;
using Domeo.Shared.Application;
using Domeo.Shared.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Modules.Domain.Entities;
using Modules.Domain.Entities.Shared;

namespace Modules.Infrastructure.Persistence;

public sealed class ModulesDbContext : DbContext, IUnitOfWork
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

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
    public DbSet<Assembly> Assemblies => Set<Assembly>();
    public DbSet<Component> Components => Set<Component>();
    public DbSet<AssemblyPart> AssemblyParts => Set<AssemblyPart>();
    public DbSet<StorageConnection> StorageConnections => Set<StorageConnection>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditInterceptor is not null)
            optionsBuilder.AddInterceptors(_auditInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("modules");

        // ModuleCategory (unchanged)
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
            builder.HasOne<ModuleCategory>().WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.SetNull);
        });

        // Assembly
        modelBuilder.Entity<Assembly>(builder =>
        {
            builder.ToTable("assemblies");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.CategoryId).HasColumnName("category_id").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            builder.Property(x => x.Parameters).HasColumnName("parameters").HasColumnType("jsonb").IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, JsonOptions)!);

            builder.Property(x => x.ParamConstraints).HasColumnName("param_constraints").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, ParamConstraint>>(v, JsonOptions));

            builder.HasIndex(x => x.Type).IsUnique();
            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.IsActive);
            builder.HasOne<ModuleCategory>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // Component
        modelBuilder.Entity<Component>(builder =>
        {
            builder.ToTable("components");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            builder.Property(x => x.Params).HasColumnName("params").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<ComponentParams>(v, JsonOptions));

            builder.Property(x => x.Color).HasColumnName("color").HasMaxLength(7);
            builder.Property(x => x.Tags).HasColumnName("tags").HasColumnType("varchar[]");

            builder.HasIndex(x => x.IsActive);
        });

        // StorageConnection
        modelBuilder.Entity<StorageConnection>(builder =>
        {
            builder.ToTable("storage_connections");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(50).IsRequired().HasDefaultValue("s3");
            builder.Property(x => x.Endpoint).HasColumnName("endpoint").HasMaxLength(500).IsRequired();
            builder.Property(x => x.Bucket).HasColumnName("bucket").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Region).HasColumnName("region").HasMaxLength(100).IsRequired();
            builder.Property(x => x.AccessKey).HasColumnName("access_key").HasMaxLength(500).IsRequired();
            builder.Property(x => x.SecretKey).HasColumnName("secret_key").HasMaxLength(500).IsRequired();
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(false);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.IsActive);
        });

        // AssemblyPart
        modelBuilder.Entity<AssemblyPart>(builder =>
        {
            builder.ToTable("assembly_parts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(x => x.AssemblyId).HasColumnName("assembly_id").IsRequired();
            builder.Property(x => x.ComponentId).HasColumnName("component_id").IsRequired();
            builder.Property(x => x.X).HasColumnName("x").HasMaxLength(500);
            builder.Property(x => x.Y).HasColumnName("y").HasMaxLength(500);
            builder.Property(x => x.Z).HasColumnName("z").HasMaxLength(500);
            builder.Property(x => x.RotationX).HasColumnName("rotation_x").IsRequired();
            builder.Property(x => x.RotationY).HasColumnName("rotation_y").IsRequired();
            builder.Property(x => x.RotationZ).HasColumnName("rotation_z").IsRequired();
            builder.Property(x => x.Condition).HasColumnName("condition").HasMaxLength(500);
            builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(1);
            builder.Property(x => x.QuantityFormula).HasColumnName("quantity_formula").HasMaxLength(200);
            builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

            builder.Property(x => x.Shape).HasColumnName("shape").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<List<ShapeSegment>>(v, JsonOptions));

            builder.Property(x => x.Provides).HasColumnName("provides").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions));

            builder.HasIndex(x => x.AssemblyId);
            builder.HasIndex(x => x.ComponentId);
            builder.HasOne<Assembly>().WithMany().HasForeignKey(x => x.AssemblyId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Component>().WithMany().HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
