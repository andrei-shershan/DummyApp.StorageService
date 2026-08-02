using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class PrintSizeService : IPrintSizeService
{
    private readonly StorageDbContext _dbContext;

    public PrintSizeService(StorageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<PrintSizeDto>> GetPrintSizesAsync()
    {
        return await _dbContext.PrintSizes
            .AsNoTracking()
            .Include(printSize => printSize.Prices)
            .OrderBy(printSize => printSize.Name)
            .Select(printSize => new PrintSizeDto
            {
                Id = printSize.Id,
                Name = printSize.Name,
                Prices = printSize.Prices
                    .Where(price => !price.IsDeleted)
                    .OrderBy(price => price.UpdatedAt)
                    .Select(price => new PriceDto
                    {
                        Id = price.Id,
                        PrintSizeId = price.PrintSizeId,
                        Value = price.Value,
                        UpdatedAt = price.UpdatedAt,
                        IsDeleted = price.IsDeleted
                    })
                    .ToList()
            })
            .ToListAsync();
    }
}
