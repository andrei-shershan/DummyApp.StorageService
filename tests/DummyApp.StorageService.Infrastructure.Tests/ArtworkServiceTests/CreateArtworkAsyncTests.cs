using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class CreateArtworkAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task WithNullRequest_ReturnsNull_AndLogsError()
    {
        await using var context = CreateContext("CreateArtworkAsync_NullRequest");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.CreateArtworkAsync(null!);

        Assert.Null(result);
        loggerMock.VerifyLog(LogLevel.Error, "Artwork create request is null.", Times.Once());
    }

    [Fact]
    public async Task WithValidArtwork_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_ValidArtwork");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = new ArtworkDto
        {
            CreatorId = "creator-1",
            Name = "Artwork name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true
        };

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.CreatorId, result!.CreatorId);
        Assert.Equal(request.Name.Trim(), result.Name);
        Assert.Equal(request.Description.Trim(), result.Description);
        Assert.Equal(request.CreationDate, result.CreationDate);
        Assert.Equal(request.UploadDate, result.UploadDate);
        Assert.Equal(request.ImgUrl.Trim(), result.ImgUrl);
        Assert.Equal(request.ThumbnailUrl.Trim(), result.ThumbnailUrl);
        Assert.Equal(request.IsActive, result.IsActive);
        Assert.NotEqual(0, result.Id);
    }
}
