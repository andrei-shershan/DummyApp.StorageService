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
    public DbSet<Series> Series { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

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

            entity.HasOne(a => a.Series)
                .WithMany(s => s.Artworks)
                .HasForeignKey(a => a.SeriesId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.CreatorId)
                .IsRequired();

            entity.HasIndex(s => new { s.CreatorId, s.Name })
                .IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.CreatedAt)
                .IsRequired();

            entity.Property(o => o.CompletedAt);

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => new { i.OrderId, i.ArtworkId });

            entity.Property(i => i.Quantity)
                .IsRequired();

            entity.HasOne(i => i.Artwork)
                .WithMany()
                .HasForeignKey(i => i.ArtworkId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
