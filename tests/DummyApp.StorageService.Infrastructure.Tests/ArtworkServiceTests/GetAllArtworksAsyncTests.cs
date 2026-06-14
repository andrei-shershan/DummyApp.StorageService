using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetAllArtworksAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task ReturnsAllArtworks()
    {
        await using var context = CreateContext("GetAllArtworksAsync_ReturnsAllArtworks");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Artworks.AddRange(
            new Artwork { Name = "Art 1", CreatorId = "c1", UploadDate = DateTime.UtcNow },
            new Artwork { Name = "Art 2", CreatorId = "c2", UploadDate = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetAllArtworksAsync();

        Assert.Collection(result,
            item => Assert.Equal("Art 1", item.Name),
            item => Assert.Equal("Art 2", item.Name));
    }
}
