using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class GetArtworksPageTests
{
    [Fact]
    public async Task ReturnsOkWithPaginatedResult()
    {
        var expected = new PaginatedResult<ArtworkDto>(
            new[] { new ArtworkDto { Id = Guid.NewGuid(), Name = "Art 1" } },
            pageNumber: 1,
            pageSize: 10,
            totalCount: 1);

        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworksPageAsync(null, null, 1, 10))
            .ReturnsAsync(expected);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworksPage(null, null, 1, 10);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
    }
}
