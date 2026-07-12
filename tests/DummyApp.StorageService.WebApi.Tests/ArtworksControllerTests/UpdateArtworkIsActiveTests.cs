using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class UpdateArtworkIsActiveTests
{
    [Fact]
    public async Task WhenRequestIsNull_ReturnsBadRequest()
    {
        var artworkService = new Mock<IArtworkService>();
        var controller = new ArtworksController(artworkService.Object);

        var artworkId = Guid.NewGuid();
        var result = await controller.UpdateArtworkIsActive(artworkId, null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Artwork request is required.", badRequest.Value);
    }

    [Fact]
    public async Task WhenArtworkDoesNotExist_ReturnsNotFound()
    {
        var artworkService = new Mock<IArtworkService>();
        var artworkId = Guid.NewGuid();
        artworkService.Setup(x => x.GetArtworkByIdAsync(artworkId, false)).ReturnsAsync((ArtworkDto?)null);

        var controller = new ArtworksController(artworkService.Object);
        var request = new UpdateArtworkIsActiveRequest { IsActive = true };

        var result = await controller.UpdateArtworkIsActive(artworkId, request);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task WhenServiceReturnsNull_ReturnsInternalServerError()
    {
        var artworkId = Guid.NewGuid();
        var existingArtwork = new ArtworkDto { Id = artworkId, CreatorId = "creator-1", Name = "Old" };
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(artworkId, false)).ReturnsAsync(existingArtwork);
        artworkService.Setup(x => x.UpdateArtworkIsActiveAsync(artworkId, true)).ReturnsAsync((ArtworkDto?)null);

        var controller = new ArtworksController(artworkService.Object);
        var request = new UpdateArtworkIsActiveRequest { IsActive = true };

        var result = await controller.UpdateArtworkIsActive(artworkId, request);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Unable to update artwork active state.", statusResult.Value);
    }

    [Fact]
    public async Task WhenUpdateSucceeds_ReturnsOkWithArtwork()
    {
        var artworkId = Guid.NewGuid();
        var existingArtwork = new ArtworkDto { Id = artworkId, CreatorId = "creator-1", Name = "Old" };
        var updatedArtwork = new ArtworkDto { Id = artworkId, CreatorId = "creator-1", Name = "Old", IsActive = true };

        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworkByIdAsync(artworkId, false)).ReturnsAsync(existingArtwork);
        artworkService.Setup(x => x.UpdateArtworkIsActiveAsync(artworkId, true)).ReturnsAsync(updatedArtwork);

        var controller = new ArtworksController(artworkService.Object);
        var request = new UpdateArtworkIsActiveRequest { IsActive = true };

        var result = await controller.UpdateArtworkIsActive(artworkId, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updatedArtwork, okResult.Value);
    }
}
