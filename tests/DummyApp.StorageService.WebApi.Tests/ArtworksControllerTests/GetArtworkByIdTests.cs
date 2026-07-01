using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class GetArtworkByIdTests
{
    [Fact]
    public async Task WhenArtworkExists_ReturnsOkWithArtwork()
    {
        var expected = new ArtworkDto { Id = 1, Name = "Art 1" };
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(1, true))
            .ReturnsAsync(expected);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworkById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task WhenArtworkDoesNotExist_ReturnsNotFound()
    {
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(1, true))
            .ReturnsAsync((ArtworkDto?)null);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworkById(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
