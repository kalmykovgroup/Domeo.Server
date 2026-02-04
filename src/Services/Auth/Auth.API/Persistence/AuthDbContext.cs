using Auth.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Persistence;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auth");

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("refresh_tokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(x => x.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
            builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(x => x.IsRevoked).HasColumnName("is_revoked").IsRequired();
            builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");
            builder.Property(x => x.LoginSessionId).HasColumnName("login_session_id");

            builder.HasIndex(x => x.Token).IsUnique();
            builder.HasIndex(x => x.UserId);
        });
    }
}
