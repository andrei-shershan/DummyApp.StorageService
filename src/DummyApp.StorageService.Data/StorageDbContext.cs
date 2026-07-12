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
    }
}
