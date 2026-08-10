using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetArtworksAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task ReturnsAllArtworks()
    {
        await using var context = CreateContext("GetArtworksAsync_ReturnsAllArtworks");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { Name = "Art 1", CreatorId = "c1", UploadDate = DateTime.UtcNow },
            new Artwork { Name = "Art 2", CreatorId = "c2", UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksAsync();

        Assert.Collection(result,
            item => Assert.Equal("Art 1", item.Name),
            item => Assert.Equal("Art 2", item.Name));
    }

    [Fact]
    public async Task ReturnsOnlyArtworksForCreator()
    {
        await using var context = CreateContext("GetArtworksAsync_ReturnsOnlyArtworksForCreator");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-2", Name = "Art 2", UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-1", Name = "Art 3", UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksAsync("creator-1", null);

        Assert.Collection(result,
            item => Assert.Equal("Art 1", item.Name),
            item => Assert.Equal("Art 3", item.Name));
    }

    [Fact]
    public async Task ReturnsEmptyList_WhenCreatorHasNoArtworks()
    {
        await using var context = CreateContext("GetArtworksAsync_ReturnsEmptyListWhenCreatorHasNoArtworks");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksAsync("creator-2", null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsEmptyList_WhenCreatorIdIsWhitespace()
    {
        await using var context = CreateContext("GetArtworksAsync_ReturnsEmptyListWhenCreatorIdIsWhitespace");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksAsync("   ", null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsOnlyActiveArtworks_WhenIsActiveFilterIsTrue()
    {
        await using var context = CreateContext("GetArtworksAsync_ReturnsOnlyActiveArtworks");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", IsActive = true, UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-1", Name = "Art 2", IsActive = false, UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksAsync("creator-1", true);

        Assert.Collection(result,
            item => Assert.Equal("Art 1", item.Name));
    }

    [Fact]
    public async Task ReturnsOnlyActiveArtworks_WhenIsActiveFilterIsTrueAndCreatorIdIsNull()
    {
        await using var context = CreateContext("GetArtworksAsync_ReturnsOnlyActiveArtworksForAllCreators");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", IsActive = true, UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-2", Name = "Art 2", IsActive = false, UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksAsync(null, true);

        Assert.Collection(result,
            item => Assert.Equal("Art 1", item.Name));
    }
}
