using DummyApp.StorageService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DummyApp.StorageService.Data;

public sealed class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options)
        : base(options)
    {
    }

    public DbSet<Artwork> Artworks { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ArtworkTag> ArtworkTags { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderAddress> OrderAddresses { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<PrintSize> PrintSizes { get; set; } = null!;
    public DbSet<Price> Prices { get; set; } = null!;
    public DbSet<VerificationCode> VerificationCodes { get; set; } = null!;
    public DbSet<CompletedOrdersToken> CompletedOrdersTokens { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("DesignTimeStorageDb");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Artwork>(entity =>
        {
            entity.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.Type)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            entity.HasIndex(t => new { t.Name, t.Type })
                .IsUnique();
        });

        modelBuilder.Entity<ArtworkTag>(entity =>
        {
            entity.HasKey(at => new { at.ArtworkId, at.TagId });

            entity.HasOne(at => at.Artwork)
                .WithMany()
                .HasForeignKey(at => at.ArtworkId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasOne(at => at.Tag)
                .WithMany(t => t.ArtworkTags)
                .HasForeignKey(at => at.TagId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasIndex(at => at.TagId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CreatedAt)
                .IsRequired();

            entity.Property(o => o.CompletedAt);

            entity.Property(o => o.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            entity.Property(o => o.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.Address)
                .WithOne(a => a.Order)
                .HasForeignKey<OrderAddress>(a => a.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderAddress>(entity =>
        {
            entity.HasKey(a => a.OrderId);

            entity.Property(a => a.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Phone)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.HouseNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(a => a.PostalCode)
                .IsRequired()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => new { i.OrderId, i.ArtworkId });

            entity.Property(i => i.Quantity)
                .IsRequired();

            entity.Property(i => i.PriceValue)
                .HasPrecision(18, 2);

            entity.HasOne(i => i.Artwork)
                .WithMany()
                .HasForeignKey(i => i.ArtworkId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.PrintSize)
                .WithMany()
                .HasForeignKey(i => i.PrintSizeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.Price)
                .WithMany()
                .HasForeignKey(i => i.PriceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(i => i.PrintSizeId);
            entity.HasIndex(i => i.PriceId);
        });

        modelBuilder.Entity<VerificationCode>(entity =>
        {
            entity.Property(v => v.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(v => v.Code)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(v => v.ExpiresAt)
                .IsRequired();

            entity.Property(v => v.IsUsed)
                .IsRequired();

            entity.Property(v => v.CreatedAt)
                .IsRequired();

            entity.HasIndex(v => new { v.Email, v.Code });
            entity.HasIndex(v => v.Email);
            entity.HasIndex(v => v.ExpiresAt);
        });

        modelBuilder.Entity<CompletedOrdersToken>(entity =>
        {
            entity.Property(c => c.Token)
                .IsRequired()
                .HasColumnType("char(36)");

            entity.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(c => c.ExpiresAt)
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            entity.HasIndex(c => c.Token)
                .IsUnique();
            entity.HasIndex(c => c.Email);
        });
    }
}
