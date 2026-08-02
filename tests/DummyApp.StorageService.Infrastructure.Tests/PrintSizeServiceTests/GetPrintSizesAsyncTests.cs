using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests.PrintSizeServiceTests;

public sealed class GetPrintSizesAsyncTests : PrintSizeServiceTestsBase
{
    [Fact]
    public async Task ReturnsPrintSizesOrderedByName_AndIncludesOnlyActivePricesOrderedByUpdatedAt()
    {
        await using var context = CreateContext("GetPrintSizesAsync_ReturnsPrintSizesOrderedByName");

        context.PrintSizes.AddRange(
            new PrintSize
            {
                Name = "B Size",
                Prices = new List<Price>
                {
                    new() { Value = 10m, UpdatedAt = DateTime.UtcNow.AddHours(-1), IsDeleted = false },
                    new() { Value = 12m, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                    new() { Value = 11m, UpdatedAt = DateTime.UtcNow.AddHours(-2), IsDeleted = true }
                }
            },
            new PrintSize
            {
                Name = "A Size",
                Prices = new List<Price>
                {
                    new() { Value = 5m, UpdatedAt = DateTime.UtcNow.AddHours(-1), IsDeleted = false }
                }
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var result = await service.GetPrintSizesAsync();

        Assert.Collection(result,
            printSize =>
            {
                Assert.Equal("A Size", printSize.Name);
                Assert.Collection(printSize.Prices,
                    price => Assert.Equal(5m, price.Value));
            },
            printSize =>
            {
                Assert.Equal("B Size", printSize.Name);
                Assert.Collection(printSize.Prices,
                    price => Assert.Equal(10m, price.Value),
                    price => Assert.Equal(12m, price.Value));
            }
        );
    }

    [Fact]
    public async Task ReturnsEmptyList_WhenNoPrintSizesExist()
    {
        await using var context = CreateContext("GetPrintSizesAsync_ReturnsEmptyListWhenNoPrintSizesExist");

        var service = CreateService(context);

        var result = await service.GetPrintSizesAsync();

        Assert.Empty(result);
    }
}
