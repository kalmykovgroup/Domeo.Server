using Microsoft.EntityFrameworkCore;
using Users.API.Entities;

namespace Users.API.Persistence;

public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
            builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Role).HasColumnName("role").HasMaxLength(50).IsRequired().HasDefaultValue("designer");
            builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Client>(builder =>
        {
            builder.ToTable("clients");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(50);
            builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255);
            builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(500);
            builder.Property(x => x.Notes).HasColumnName("notes");
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.HasIndex(x => x.UserId);
            builder.HasQueryFilter(x => x.DeletedAt == null);
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
