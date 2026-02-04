using Audit.Abstractions.Entities;
using Domeo.Shared.Application;
using Microsoft.EntityFrameworkCore;

namespace Audit.API.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext, IUnitOfWork
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<LoginSession> LoginSessions => Set<LoginSession>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("audit");

        modelBuilder.Entity<LoginSession>(builder =>
        {
            builder.ToTable("login_sessions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.UserRole).HasColumnName("user_role").HasMaxLength(50).IsRequired();
            builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            builder.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(500);
            builder.Property(x => x.LoggedInAt).HasColumnName("logged_in_at").IsRequired();
            builder.Property(x => x.LoggedOutAt).HasColumnName("logged_out_at");

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.LoggedInAt);
            builder.Ignore(x => x.IsActive);
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("audit_logs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
            builder.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
            builder.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(100).IsRequired();
            builder.Property(x => x.ServiceName).HasColumnName("service_name").HasMaxLength(100).IsRequired();
            builder.Property(x => x.OldValue).HasColumnName("old_value").HasColumnType("jsonb");
            builder.Property(x => x.NewValue).HasColumnName("new_value").HasColumnType("jsonb");
            builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(50);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.EntityType);
            builder.HasIndex(x => new { x.EntityType, x.EntityId });
            builder.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<ApplicationLog>(builder =>
        {
            builder.ToTable("application_logs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.ServiceName).HasColumnName("service_name").HasMaxLength(100).IsRequired();
            builder.Property(x => x.Level).HasColumnName("level").HasMaxLength(20).IsRequired();
            builder.Property(x => x.Message).HasColumnName("message").IsRequired();
            builder.Property(x => x.Exception).HasColumnName("exception");
            builder.Property(x => x.ExceptionType).HasColumnName("exception_type").HasMaxLength(500);
            builder.Property(x => x.Properties).HasColumnName("properties").HasColumnType("jsonb");
            builder.Property(x => x.RequestPath).HasColumnName("request_path").HasMaxLength(500);
            builder.Property(x => x.UserId).HasColumnName("user_id");
            builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(100);
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

            builder.HasIndex(x => x.ServiceName);
            builder.HasIndex(x => x.Level);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.UserId);
        });
    }
}
