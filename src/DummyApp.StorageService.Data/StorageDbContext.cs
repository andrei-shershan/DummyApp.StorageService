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
    }
}
