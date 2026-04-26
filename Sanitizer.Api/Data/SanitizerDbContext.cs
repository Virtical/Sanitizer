using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Data.Entities;

namespace Sanitizer.Api.Data;

public class SanitizerDbContext(DbContextOptions<SanitizerDbContext> options) : DbContext(options)
{
    public DbSet<SanitizationProfileEntity> Profiles => Set<SanitizationProfileEntity>();
    public DbSet<SanitizationRuleEntity> Rules => Set<SanitizationRuleEntity>();
    public DbSet<ApiKeyEntity> ApiKeys => Set<ApiKeyEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SanitizationProfileEntity>()
            .HasMany(p => p.Rules)
            .WithOne(r => r.Profile)
            .HasForeignKey(r => r.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SanitizationRuleEntity>()
            .HasIndex(r => new { r.ProfileId, r.DetectorType })
            .IsUnique();

        modelBuilder.Entity<ApiKeyEntity>()
            .HasIndex(k => k.KeyHash)
            .IsUnique(false);
    }
}
