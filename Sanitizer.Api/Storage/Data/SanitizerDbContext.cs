using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage.Data;

public class SanitizerDbContext(DbContextOptions<SanitizerDbContext> options) : DbContext(options)
{
    public DbSet<SanitizationProfileEntity> Profiles => Set<SanitizationProfileEntity>();
    public DbSet<SanitizationRuleEntity> Rules => Set<SanitizationRuleEntity>();
    public DbSet<ApiKeyEntity> ApiKeys => Set<ApiKeyEntity>();
    public DbSet<ChatEntity> Chats => Set<ChatEntity>();
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

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

        modelBuilder.Entity<ChatEntity>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MessageEntity>()
            .HasIndex(m => new { m.ChatId, m.OrderIndex })
            .IsUnique();
    }
}
