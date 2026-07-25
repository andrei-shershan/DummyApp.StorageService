using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Authorization;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetArtworkByIdAsyncTests : ArtworkServiceTestsBase
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public async Task WhenActiveOnlyTrue_ReturnsOnlyActiveArtwork(bool isActive, bool expectedFound)
    {
        await using var context = CreateContext("GetArtworkByIdAsync_WhenActiveOnlyTrue");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var artwork = new Artwork
        {
            CreatorId = "creator-2",
            Name = "Artwork",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = isActive
        };
        context.Artworks.Add(artwork);
        await context.SaveChangesAsync();

        var result = await service.GetArtworkByIdAsync(artwork.Id, true);

        if (expectedFound)
        {
            Assert.NotNull(result);
            Assert.Equal(artwork.Id, result!.Id);
        }
        else
        {
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task WhenActiveOnlyFalseAndArtworkInactive_ReturnsArtwork()
    {
        await using var context = CreateContext("GetArtworkByIdAsync_WhenActiveOnlyFalse");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var artwork = new Artwork
        {
            CreatorId = "creator-2",
            Name = "Inactive artwork",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = false
        };
        context.Artworks.Add(artwork);
        await context.SaveChangesAsync();

        var result = await service.GetArtworkByIdAsync(artwork.Id, false);

        Assert.NotNull(result);
        Assert.Equal(artwork.Id, result!.Id);
    }

    [Fact]
    public async Task WhenArtworkDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext("GetArtworkByIdAsync_WhenArtworkDoesNotExist");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworkByIdAsync(Guid.NewGuid(), true);

        Assert.Null(result);
    }
}
