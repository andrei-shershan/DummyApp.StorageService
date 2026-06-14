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
        context.Artworks.Add(new Artwork
        {
            CreatorId = "creator-2",
            Name = "Existing artwork",
            PublicName = "Public name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            SmallImgUrl = "https://example.com/small.jpg",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var result = await service.GetArtworkByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Existing artwork", result.Name);
    }

    [Fact]
    public async Task WhenArtworkDoesNotExist_ReturnsNull()
    {
        await using var context = CreateContext("GetArtworkByIdAsync_WhenArtworkDoesNotExist");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworkByIdAsync(999);

        Assert.Null(result);
    }
}
