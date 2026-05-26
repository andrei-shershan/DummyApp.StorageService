using DummyApp.StorageService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DummyApp.StorageService.Data;

public sealed class StorageDbContext : DbContext
{
    public StorageDbContext(DbContextOptions<StorageDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<MessageType> MessageTypes { get; set; } = null!;
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

        modelBuilder.Entity<MessageType>(entity =>
        {
            entity.ToTable("MessageTypes");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Text)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(m => m.MessageTypeId)
                .HasColumnType("int");
            entity.HasOne(m => m.MessageType)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.MessageTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Artwork>(entity =>
        {
            entity.ToTable("Artworks");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.CreatorId)
                .IsRequired();
            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(a => a.Description)
                .HasMaxLength(255);
            entity.Property(a => a.CreationDate)
                .IsRequired();
            entity.Property(a => a.UploadDate)
                .IsRequired();
            entity.Property(a => a.ImgUrl)
                .IsRequired()
                .HasMaxLength(2000);
            entity.Property(a => a.SmallImgUrl)
                .HasMaxLength(2000);
            entity.Property(a => a.IsActive)
                .IsRequired();
        });
    }
}
