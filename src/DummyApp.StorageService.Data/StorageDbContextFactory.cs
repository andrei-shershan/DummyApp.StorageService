using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DummyApp.StorageService.Data;

public sealed class StorageDbContextFactory : IDesignTimeDbContextFactory<StorageDbContext>
{
    public StorageDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StorageDbContext>();
        optionsBuilder.UseMySQL(
            "server=localhost;port=3306;database=dummy_db;user=dummyapp;password=secret",
            sql => sql.MigrationsAssembly("DummyApp.StorageService.Data"));
        return new StorageDbContext(optionsBuilder.Options);
    }
}
