using System;
using System.Linq;
using System.Threading.Tasks;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class SeriesTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task CreateSeriesAsync_ReturnsNull_WhenCreatorIdIsMissing()
    {
        await using var context = CreateContext("CreateSeriesAsync_ReturnsNull_WhenCreatorIdIsMissing");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.CreateSeriesAsync(string.Empty, "Series name");

        Assert.Null(result);
        loggerMock.VerifyLog(LogLevel.Error, "Series create request does not contain creator id.", Times.Once());
    }

    [Fact]
    public async Task CreateSeriesAsync_ReturnsNull_WhenNameIsWhitespace()
    {
        await using var context = CreateContext("CreateSeriesAsync_ReturnsNull_WhenNameIsWhitespace");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.CreateSeriesAsync("creator-1", "   ");

        Assert.Null(result);
        loggerMock.VerifyLog(LogLevel.Error, "Series create request does not contain a valid name.", Times.Once());
    }

    [Fact]
    public async Task CreateSeriesAsync_CreatesNewSeries_WhenValidRequest()
    {
        await using var context = CreateContext("CreateSeriesAsync_CreatesNewSeries_WhenValidRequest");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var result = await service.CreateSeriesAsync("creator-1", "New Series");

        Assert.NotNull(result);
        Assert.Equal("creator-1", result!.CreatorId);
        Assert.Equal("New Series", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);

        var storedSeries = context.Series.FirstOrDefault();
        Assert.NotNull(storedSeries);
        Assert.Equal(result.Id, storedSeries!.Id);
        Assert.Equal("New Series", storedSeries.Name);
    }

    [Fact]
    public async Task CreateSeriesAsync_ReturnsExistingSeries_WhenSeriesAlreadyExists()
    {
        await using var context = CreateContext("CreateSeriesAsync_ReturnsExistingSeries_WhenSeriesAlreadyExists");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Series.Add(new Series { CreatorId = "creator-1", Name = "Existing Series" });
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.CreateSeriesAsync("creator-1", "Existing Series");

        Assert.NotNull(result);
        Assert.Equal("creator-1", result!.CreatorId);
        Assert.Equal("Existing Series", result.Name);
        Assert.Single(context.Series);
    }

    [Fact]
    public async Task GetSeriesByCreatorAsync_ReturnsOrderedSeries_WhenCreatorHasSeries()
    {
        await using var context = CreateContext("GetSeriesByCreatorAsync_ReturnsOrderedSeries_WhenCreatorHasSeries");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        context.Series.AddRange(
            new Series { CreatorId = "creator-1", Name = "Z Series" },
            new Series { CreatorId = "creator-1", Name = "A Series" },
            new Series { CreatorId = "creator-2", Name = "Other Series" }
        );
        await context.SaveChangesAsync();

        var service = CreateService(context, loggerMock.Object);

        var result = await service.GetSeriesByCreatorAsync("creator-1");

        Assert.Collection(result,
            first => Assert.Equal("A Series", first.Name),
            second => Assert.Equal("Z Series", second.Name));
    }

    [Fact]
    public async Task CreateArtworkAsync_WithSeriesName_CreatesSeriesAndAssignsIt()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithSeriesName_CreatesSeriesAndAssignsIt");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);

        var artworkRequest = new ArtworkDto
        {
            CreatorId = "creator-1",
            Name = "Artwork with series",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "img",
            ThumbnailUrl = "thumb",
            IsActive = true,
            SeriesName = "Series Alpha"
        };

        var result = await service.CreateArtworkAsync(artworkRequest);

        Assert.NotNull(result);
        Assert.Equal("Series Alpha", result!.SeriesName);
        Assert.Single(context.Series);
        Assert.Single(context.Artworks);
    }
}
