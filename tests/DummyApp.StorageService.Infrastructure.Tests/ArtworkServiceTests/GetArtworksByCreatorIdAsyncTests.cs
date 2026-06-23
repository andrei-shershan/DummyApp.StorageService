using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetArtworksByCreatorIdAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task ReturnsOnlyArtworksForCreator()
    {
        await using var context = CreateContext("GetArtworksByCreatorIdAsync_ReturnsOnlyArtworksForCreator");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-2", Name = "Art 2", UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-1", Name = "Art 3", UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksByCreatorIdAsync("creator-1");

        Assert.Collection(result,
            item => Assert.Equal("Art 1", item.Name),
            item => Assert.Equal("Art 3", item.Name));
    }

    [Fact]
    public async Task ReturnsEmptyList_WhenCreatorHasNoArtworks()
    {
        await using var context = CreateContext("GetArtworksByCreatorIdAsync_ReturnsEmptyListWhenCreatorHasNoArtworks");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksByCreatorIdAsync("creator-2");

        Assert.Empty(result);
    }

    [Fact]
    public async Task ReturnsEmptyList_WhenCreatorIdIsWhitespace()
    {
        await using var context = CreateContext("GetArtworksByCreatorIdAsync_ReturnsEmptyListWhenCreatorIdIsWhitespace");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksByCreatorIdAsync("   ");

        Assert.Empty(result);
    }
}
