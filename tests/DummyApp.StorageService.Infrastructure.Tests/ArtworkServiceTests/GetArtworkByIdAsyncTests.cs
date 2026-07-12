using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetArtworkByIdAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task WhenArtworkExists_ReturnsArtwork()
    {
        await using var context = CreateContext("GetArtworkByIdAsync_WhenArtworkExists");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var artwork = new Artwork
        {
            CreatorId = "creator-2",
            Name = "Existing artwork",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true
        };
        context.Artworks.Add(artwork);
        await context.SaveChangesAsync();

        var result = await service.GetArtworkByIdAsync(artwork.Id, true);

        Assert.NotNull(result);
        Assert.Equal(artwork.Id, result!.Id);
        Assert.Equal("Existing artwork", result.Name);
    }

    [Fact]
    public async Task WhenInactiveArtworkAndActiveOnlyFalse_ReturnsArtwork()
    {
        await using var context = CreateContext("GetArtworkByIdAsync_WhenInactiveArtworkActiveOnlyFalse");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        context.Artworks.Add(new Artwork
        {
            CreatorId = "creator-2",
            Name = "Inactive artwork",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = false
        });
        await context.SaveChangesAsync();

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
        Assert.Equal("Inactive artwork", result.Name);
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
