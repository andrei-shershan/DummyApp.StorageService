using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IPrintSizeService
{
    Task<IEnumerable<PrintSizeDto>> GetPrintSizesAsync();
}
