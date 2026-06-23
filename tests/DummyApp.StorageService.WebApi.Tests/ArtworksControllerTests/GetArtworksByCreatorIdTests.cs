using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class GetArtworksByCreatorIdTests
{
    [Fact]
    public async Task ReturnsOkWithArtworkList()
    {
        var creatorId = "8bc3c2c6-1e90-4a84-bd78-1c5fae9f3d3a";
        var expected = new[]
        {
            new ArtworkDto { Id = 1, CreatorId = creatorId, Name = "Art 1" },
            new ArtworkDto { Id = 2, CreatorId = creatorId, Name = "Art 2" }
        };

        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworksByCreatorIdAsync(creatorId))
            .ReturnsAsync(expected);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworksByCreatorId(creatorId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ReturnsBadRequest_WhenCreatorIdIsMissing()
    {
        var artworkService = new Mock<IArtworkService>();
        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworksByCreatorId(string.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
