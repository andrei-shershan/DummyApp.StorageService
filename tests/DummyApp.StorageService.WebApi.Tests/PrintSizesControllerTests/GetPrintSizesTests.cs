using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.PrintSizesControllerTests;

public sealed class GetPrintSizesTests
{
    [Fact]
    public async Task GetPrintSizes_ReturnsOkWithPrintSizes()
    {
        var printSizes = new[]
        {
            new PrintSizeDto { Id = 1, Name = "A4", Prices = Array.Empty<PriceDto>() },
            new PrintSizeDto { Id = 2, Name = "A3", Prices = Array.Empty<PriceDto>() }
        };

        var printSizeServiceMock = new Mock<IPrintSizeService>();
        printSizeServiceMock.Setup(x => x.GetPrintSizesAsync())
            .ReturnsAsync(printSizes);

        var controller = new PrintSizesController(printSizeServiceMock.Object);

        var result = await controller.GetPrintSizes();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsAssignableFrom<IEnumerable<PrintSizeDto>>(okResult.Value);
        Assert.Equal(printSizes, actual);
        printSizeServiceMock.Verify(x => x.GetPrintSizesAsync(), Times.Once);
    }
}
