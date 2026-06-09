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
    public DbSet<UserEntity> Users => Set<UserEntity>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SanitizationProfileEntity>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.ApiKeyId).IsRequired();
            b.HasIndex(p => p.ApiKeyId);

            b.HasMany(p => p.Rules)
                .WithOne(r => r.Profile)
                .HasForeignKey(r => r.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SanitizationRuleEntity>(b =>
        {
            b.HasIndex(r => new { r.ProfileId, r.DetectorType }).IsUnique();
        });

        modelBuilder.Entity<ApiKeyEntity>(b =>
        {
            b.HasKey(k => k.Id);
            b.HasIndex(k => k.IsActive);
        });

        modelBuilder.Entity<ChatEntity>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.ApiKeyId).IsRequired();
            b.HasIndex(c => c.ApiKeyId);

            b.HasMany(c => c.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MessageEntity>(b =>
        {
            b.HasIndex(m => new { m.ChatId, m.OrderIndex }).IsUnique();
        });
        
        modelBuilder.Entity<UserEntity>(b =>
        {
            b.HasOne<ApiKeyEntity>()
                .WithMany()
                .HasForeignKey(u => u.ApiKeyId)
                .IsRequired();
        });

    }
}
