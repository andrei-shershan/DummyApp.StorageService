using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetArtworksPageAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task ReturnsCorrectPage_WithTotalCount()
    {
        await using var context = CreateContext("GetArtworksPageAsync_ReturnsCorrectPage");
        var loggerMock = new Mock<ILogger<ArtworkService>>();

        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-2", Name = "Art 2", UploadDate = DateTime.UtcNow.AddMinutes(1) },
            new Artwork { CreatorId = "creator-3", Name = "Art 3", UploadDate = DateTime.UtcNow.AddMinutes(2) },
            new Artwork { CreatorId = "creator-4", Name = "Art 4", UploadDate = DateTime.UtcNow.AddMinutes(3) }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksPageAsync(null, null, pageNumber: 2, pageSize: 2);

        Assert.Equal(2, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(4, result.TotalCount);
        Assert.Collection(result.Items,
            item => Assert.Equal("Art 3", item.Name),
            item => Assert.Equal("Art 4", item.Name));
    }

    [Fact]
    public async Task ReturnsEmptyPage_WhenPageNumberExceedsTotal()
    {
        await using var context = CreateContext("GetArtworksPageAsync_ReturnsEmptyPageWhenPageTooHigh");
        var loggerMock = new Mock<ILogger<ArtworkService>>();

        context.Artworks.AddRange(
            new Artwork { CreatorId = "creator-1", Name = "Art 1", UploadDate = DateTime.UtcNow },
            new Artwork { CreatorId = "creator-2", Name = "Art 2", UploadDate = DateTime.UtcNow.AddMinutes(1) }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetArtworksPageAsync(null, null, pageNumber: 3, pageSize: 2);

        Assert.Equal(3, result.PageNumber);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalCount);
        Assert.Empty(result.Items);
    }
}
