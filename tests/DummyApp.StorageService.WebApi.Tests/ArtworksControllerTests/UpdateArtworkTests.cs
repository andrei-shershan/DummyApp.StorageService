using System.Security.Claims;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class UpdateArtworkTests
{
    [Fact]
    public async Task WhenRequestIsNull_ReturnsBadRequest()
    {
        var artworkService = new Mock<IArtworkService>();
        var controller = new ArtworksController(artworkService.Object);
        var artworkId = Guid.NewGuid();

        var result = await controller.UpdateArtwork(artworkId, null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Artwork request is required.", badRequest.Value);
    }

    [Fact]
    public async Task WhenArtworkDoesNotExist_ReturnsNotFound()
    {
        var artworkId = Guid.NewGuid();
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(artworkId, false)).ReturnsAsync((ArtworkDto?)null);

        var controller = new ArtworksController(artworkService.Object);
        var request = new UpdateArtworkDto { Name = "Test" };

        var result = await controller.UpdateArtwork(artworkId, request);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task WhenServiceReturnsNull_ReturnsInternalServerError()
    {
        var artworkId = Guid.NewGuid();
        var existingArtwork = new ArtworkDto { Id = artworkId, CreatorId = "creator-1", Name = "Old" };
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(artworkId, false)).ReturnsAsync(existingArtwork);
        artworkService.Setup(x => x.UpdateArtworkAsync(artworkId, It.IsAny<UpdateArtworkDto>()))
            .ReturnsAsync((ArtworkDto?)null);

        var controller = new ArtworksController(artworkService.Object);
        var request = new UpdateArtworkDto { Name = "Updated" };

        var result = await controller.UpdateArtwork(artworkId, request);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Unable to update artwork.", statusResult.Value);
    }

    [Fact]
    public async Task WhenUpdateSucceeds_ReturnsOkWithArtwork()
    {
        var artworkId = Guid.NewGuid();
        var existingArtwork = new ArtworkDto { Id = artworkId, CreatorId = "creator-1", Name = "Old" };
        var updatedArtwork = new ArtworkDto { Id = artworkId, CreatorId = "creator-1", Name = "Updated" };

        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(artworkId, false)).ReturnsAsync(existingArtwork);
        artworkService.Setup(x => x.UpdateArtworkAsync(artworkId, It.IsAny<UpdateArtworkDto>())).ReturnsAsync(updatedArtwork);

        var controller = new ArtworksController(artworkService.Object);
        var request = new UpdateArtworkDto { Name = "Updated" };

        var result = await controller.UpdateArtwork(artworkId, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updatedArtwork, okResult.Value);
    }

}
