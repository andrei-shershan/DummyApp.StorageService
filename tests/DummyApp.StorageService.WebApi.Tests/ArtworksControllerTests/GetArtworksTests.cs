using System.Security.Claims;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.ArtworksControllerTests;

public sealed class GetArtworksTests
{
    [Fact]
    public async Task ReturnsOkWithArtworkList()
    {
        var expected = new[]
        {
            new ArtworkDto { Id = 1, Name = "Art 1" },
            new ArtworkDto { Id = 2, Name = "Art 2" }
        };

        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworksAsync(null, null))
            .ReturnsAsync(expected);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworks(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ReturnsOkWithArtworkList_WhenCreatorIdProvided()
    {
        var creatorId = "creator-1";
        var expected = new[]
        {
            new ArtworkDto { Id = 1, CreatorId = creatorId, Name = "Art 1" },
            new ArtworkDto { Id = 2, CreatorId = creatorId, Name = "Art 2" }
        };

        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworksAsync(creatorId, null))
            .ReturnsAsync(expected);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworks(creatorId, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task ReturnsOkWithEmptyList_WhenNoArtworks()
    {
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworksAsync(null, null))
            .ReturnsAsync(Array.Empty<ArtworkDto>());

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworks(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(Array.Empty<ArtworkDto>(), okResult.Value);
    }

    [Fact]
    public async Task ReturnsOkWithEmptyList_WhenServiceReturnsNull()
    {
        var artworkService = new Mock<IArtworkService>();
        artworkService.Setup(x => x.GetArtworksAsync(null, null))
            .ReturnsAsync((IEnumerable<ArtworkDto>?)null!);

        var controller = new ArtworksController(artworkService.Object);

        var result = await controller.GetArtworks(null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(Array.Empty<ArtworkDto>(), okResult.Value);
    }

}
