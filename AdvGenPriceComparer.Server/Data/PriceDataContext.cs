using AdvGenPriceComparer.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvGenPriceComparer.Server.Data;

/// <summary>
/// Entity Framework database context for the price sharing server
/// </summary>
public class PriceDataContext : DbContext
{
    public PriceDataContext(DbContextOptions<PriceDataContext> options) : base(options)
    {
    }

    /// <summary>
    /// Shared product items
    /// </summary>
    public DbSet<SharedItem> Items { get; set; } = null!;

    /// <summary>
    /// Store locations
    /// </summary>
    public DbSet<SharedPlace> Places { get; set; } = null!;

    /// <summary>
    /// Price records submitted by users
    /// </summary>
    public DbSet<SharedPriceRecord> PriceRecords { get; set; } = null!;

    /// <summary>
    /// API keys for client authentication
    /// </summary>
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;

    /// <summary>
    /// User upload sessions
    /// </summary>
    public DbSet<UploadSession> UploadSessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SharedItem
        modelBuilder.Entity<SharedItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
            entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Brand).HasMaxLength(200);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        // Configure SharedPlace
        modelBuilder.Entity<SharedPlace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Chain);
            entity.HasIndex(e => e.State);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Chain).HasMaxLength(100);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Suburb).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.Postcode).HasMaxLength(20);
        });

        // Configure SharedPriceRecord
        modelBuilder.Entity<SharedPriceRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.PlaceId);
            entity.HasIndex(e => e.DateRecorded);
            entity.HasIndex(e => e.IsCurrent);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.OriginalPrice).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("AUD");
            entity.HasOne(e => e.Item)
                .WithMany(i => i.PriceRecords)
                .HasForeignKey(e => e.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Place)
                .WithMany(p => p.PriceRecords)
                .HasForeignKey(e => e.PlaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ApiKey
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.KeyHash).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.KeyHash).HasMaxLength(256).IsRequired();
            entity.Property(e => e.RateLimit).HasDefaultValue(100);
        });

        // Configure UploadSession
        modelBuilder.Entity<UploadSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ApiKeyId);
            entity.HasIndex(e => e.UploadedAt);
            entity.Property(e => e.ClientVersion).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
        });
    }
}
