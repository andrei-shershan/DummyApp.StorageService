using System.Security.Claims;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class CreateArtworkTests
{
    [Fact]
    public async Task WhenRequestIsNull_ReturnsBadRequest()
    {
        var artworkService = new Mock<IArtworkService>();
        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.CreateArtwork(null!);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Artwork request is required.", badRequest.Value);
    }

    [Fact]
    public async Task WhenModelStateIsInvalid_ReturnsBadRequest()
    {
        var artworkService = new Mock<IArtworkService>();
        var controller = new ArtworksController(artworkService.Object);
        controller.ModelState.AddModelError("Name", "Required");

        var request = new ArtworkDto { Name = string.Empty };

        var result = await controller.CreateArtwork(request);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.IsType<SerializableError>(badRequest.Value);
        Assert.Contains("Name", ((SerializableError)badRequest.Value).Keys);
    }

    [Fact]
    public async Task WhenServiceReturnsNull_ReturnsInternalServerError()
    {
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.CreateArtworkAsync(It.IsAny<ArtworkDto>()))
            .ReturnsAsync((ArtworkDto?)null);

        var controller = new ArtworksController(artworkService.Object);
        var request = new ArtworkDto { Name = "Test" };

        var result = await controller.CreateArtwork(request);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Unable to create artwork.", statusResult.Value);
    }

    [Fact]
    public async Task WhenArtworkCreated_ReturnsCreatedAtAction()
    {
        var expected = new ArtworkDto { Id = 1, Name = "Test" };
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.CreateArtworkAsync(It.IsAny<ArtworkDto>()))
            .ReturnsAsync(expected);

        var controller = new ArtworksController(artworkService.Object);
        var request = new ArtworkDto { Name = "Test" };

        var result = await controller.CreateArtwork(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ArtworksController.GetArtworkById), createdResult.ActionName);
        Assert.Equal(expected.Id, ((dynamic)createdResult.RouteValues!)![("id")]);
        Assert.Equal(expected, createdResult.Value);
    }

}
