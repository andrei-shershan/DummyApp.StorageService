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
        var request = new CreateArtworkRequest
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
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task WithNewTagMatchingExistingTag_ReturnsNull_AndLogsWarning()
    {
        await using var context = CreateContext("CreateArtworkAsync_NewTagMatchesExistingTag");
        var existingTag = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "Existing Tag",
            Type = DummyApp.StorageService.Data.Models.TagType.None
        };

        context.Tags.Add(existingTag);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = new CreateArtworkRequest
        {
            CreatorId = "creator-1",
            Name = "Artwork name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true,
            NewTags = new[]
            {
                new NewTagRequest { Name = "Existing Tag", Type = "None" }
            }
        };

        var result = await service.CreateArtworkAsync(request);

        Assert.Null(result);
        loggerMock.VerifyLog(LogLevel.Warning, "Artwork create request contains a new tag that already exists", Times.Once());
    }

    [Fact]
    public async Task WithExistingSeriesTag_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithExistingSeriesTag");
        var existingTag = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "Series Tag",
            Type = DummyApp.StorageService.Data.Models.TagType.Series
        };

        context.Tags.Add(existingTag);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = new CreateArtworkRequest
        {
            CreatorId = "creator-1",
            Name = "Artwork name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true,
            ExistingTagIds = new[] { existingTag.Id }
        };

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == existingTag.Id);
    }

    [Fact]
    public async Task WithExistingSeriesTagAsList_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithExistingSeriesTagAsList");
        var existingTag = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "Series Tag",
            Type = DummyApp.StorageService.Data.Models.TagType.Series
        };

        context.Tags.Add(existingTag);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = new CreateArtworkRequest
        {
            CreatorId = "creator-1",
            Name = "Artwork name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true,
            ExistingTagIds = new List<Guid> { existingTag.Id }
        };

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == existingTag.Id);
    }

    [Fact]
    public async Task WithMultipleExistingTags_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithMultipleExistingTags");
        var existingTag1 = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "Series Tag 1",
            Type = DummyApp.StorageService.Data.Models.TagType.Series
        };
        var existingTag2 = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "None Tag 2",
            Type = DummyApp.StorageService.Data.Models.TagType.None
        };

        context.Tags.AddRange(existingTag1, existingTag2);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = new CreateArtworkRequest
        {
            CreatorId = "creator-1",
            Name = "Artwork name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true,
            ExistingTagIds = new[] { existingTag1.Id, existingTag2.Id }
        };

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == existingTag1.Id);
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == existingTag2.Id);
        Assert.Equal(2, context.ArtworkTags.Count(at => at.ArtworkId == result.Id));
    }

    [Fact]
    public async Task WithNewSeriesTag_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithNewSeriesTag");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = CreateValidArtworkRequest(newTags: new[]
        {
            new NewTagRequest { Name = "Series 1", Type = "Series" }
        });

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Contains(context.Tags, tag => tag.Name == "Series 1" && tag.Type == DummyApp.StorageService.Data.Models.TagType.Series);
        var seriesTag = context.Tags.First(tag => tag.Name == "Series 1");
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == seriesTag.Id);
    }

    [Fact]
    public async Task WithNewNoneTag_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithNewNoneTag");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = CreateValidArtworkRequest(newTags: new[]
        {
            new NewTagRequest { Name = "Tag 1", Type = "None" }
        });

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Contains(context.Tags, tag => tag.Name == "Tag 1" && tag.Type == DummyApp.StorageService.Data.Models.TagType.None);
        var noneTag = context.Tags.First(tag => tag.Name == "Tag 1");
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == noneTag.Id);
    }

    [Fact]
    public async Task WithNewSeriesAndNewTag_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithNewSeriesAndNewTag");
        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = CreateValidArtworkRequest(newTags: new[]
        {
            new NewTagRequest { Name = "Series 2", Type = "Series" },
            new NewTagRequest { Name = "Tag 2", Type = "None" }
        });

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Equal(2, context.Tags.Count());
        Assert.Equal(2, context.ArtworkTags.Count(at => at.ArtworkId == result.Id));
    }

    [Fact]
    public async Task WithExistingSeriesAndNewNoneTag_SavesAndReturnsArtwork()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithExistingSeriesAndNewNoneTag");
        var existingSeriesTag = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "Existing Series",
            Type = DummyApp.StorageService.Data.Models.TagType.Series
        };

        context.Tags.Add(existingSeriesTag);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = CreateValidArtworkRequest(
            existingTagIds: new[] { existingSeriesTag.Id },
            newTags: new[]
            {
                new NewTagRequest { Name = "Tag 3", Type = "None" }
            });

        var result = await service.CreateArtworkAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Equal(2, context.ArtworkTags.Count(at => at.ArtworkId == result.Id));
        Assert.Contains(context.ArtworkTags, at => at.ArtworkId == result.Id && at.TagId == existingSeriesTag.Id);
        Assert.Contains(context.Tags, tag => tag.Name == "Tag 3" && tag.Type == DummyApp.StorageService.Data.Models.TagType.None);
    }

    [Fact]
    public async Task WithNewSeriesWhenExistingSeriesExists_ReturnsNull_AndLogsWarning()
    {
        await using var context = CreateContext("CreateArtworkAsync_WithNewSeriesWhenExistingSeriesExists");
        var existingSeriesTag = new DummyApp.StorageService.Data.Models.Tag
        {
            Name = "Existing Series",
            Type = DummyApp.StorageService.Data.Models.TagType.Series
        };

        context.Tags.Add(existingSeriesTag);
        await context.SaveChangesAsync();

        var loggerMock = new Mock<ILogger<ArtworkService>>();
        var service = CreateService(context, loggerMock.Object);
        var request = CreateValidArtworkRequest(
            existingTagIds: new[] { existingSeriesTag.Id },
            newTags: new[]
            {
                new NewTagRequest { Name = "Series 3", Type = "Series" }
            });

        var result = await service.CreateArtworkAsync(request);

        Assert.Null(result);
        loggerMock.VerifyLog(LogLevel.Warning, "Artwork create request contains more than one series tag.", Times.Once());
    }

    private static CreateArtworkRequest CreateValidArtworkRequest(
        IEnumerable<Guid>? existingTagIds = null,
        IEnumerable<NewTagRequest>? newTags = null)
    {
        return new CreateArtworkRequest
        {
            CreatorId = "creator-1",
            Name = "Artwork name",
            Description = "Description",
            CreationDate = new DateTime(2024, 1, 1),
            UploadDate = DateTime.UtcNow,
            ImgUrl = "https://example.com/img.jpg",
            ThumbnailUrl = "https://example.com/small.jpg",
            IsActive = true,
            ExistingTagIds = existingTagIds,
            NewTags = newTags
        };
    }
}
