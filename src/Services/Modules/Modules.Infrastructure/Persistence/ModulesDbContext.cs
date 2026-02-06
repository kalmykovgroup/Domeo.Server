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

            builder.Property(x => x.Dimensions).HasColumnName("dimensions").HasColumnType("jsonb").IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<Dimensions>(v, JsonOptions)!);

            builder.Property(x => x.Constraints).HasColumnName("constraints").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<Constraints>(v, JsonOptions));

            builder.Property(x => x.Construction).HasColumnName("construction").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<Construction>(v, JsonOptions));

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

            builder.Property(x => x.Tags).HasColumnName("tags").HasColumnType("varchar[]");

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
            builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(50).IsRequired()
                .HasConversion<string>();
            builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(1);
            builder.Property(x => x.QuantityFormula).HasColumnName("quantity_formula").HasMaxLength(200);
            builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();

            builder.Property(x => x.Length).HasColumnName("length").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<DynamicSize>(v, JsonOptions));

            builder.Property(x => x.Width).HasColumnName("width").HasColumnType("jsonb")
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, JsonOptions),
                    v => v == null ? null : JsonSerializer.Deserialize<DynamicSize>(v, JsonOptions));

            builder.Property(x => x.Placement).HasColumnName("placement").HasColumnType("jsonb").IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<Placement>(v, JsonOptions)!);

            builder.HasIndex(x => x.AssemblyId);
            builder.HasIndex(x => x.ComponentId);
            builder.HasOne<Assembly>().WithMany().HasForeignKey(x => x.AssemblyId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Component>().WithMany().HasForeignKey(x => x.ComponentId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
