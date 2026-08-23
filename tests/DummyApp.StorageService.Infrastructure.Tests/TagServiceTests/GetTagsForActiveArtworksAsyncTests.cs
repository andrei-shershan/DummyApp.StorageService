using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Xunit;

namespace DummyApp.StorageService.Infrastructure.Tests;

public sealed class GetTagsForActiveArtworksAsyncTests : ArtworkServiceTestsBase
{
    [Fact]
    public async Task ReturnsTagsForActiveArtworks_OnlyNoneAndSeriesSortedByName()
    {
        await using var context = CreateContext("GetTagsForActiveArtworksAsync_ReturnsTagsForActiveArtworks");
        var service = new TagService(context);

        var activeArtwork = new Artwork
        {
            CreatorId = "creator-1",
            Name = "Active Art",
            UploadDate = DateTime.UtcNow,
            IsActive = true
        };

        var inactiveArtwork = new Artwork
        {
            CreatorId = "creator-1",
            Name = "Inactive Art",
            UploadDate = DateTime.UtcNow.AddMinutes(1),
            IsActive = false
        };

        var activeNoneTag = new Tag { Name = "A None Tag", Type = TagType.None };
        var activeSeriesTag = new Tag { Name = "B Series Tag", Type = TagType.Series };
        var inactiveTag = new Tag { Name = "C Inactive Tag", Type = TagType.None };
        var commerceTag = new Tag { Name = "D Commerce Tag", Type = TagType.Commerce };

        context.Artworks.AddRange(activeArtwork, inactiveArtwork);
        context.Tags.AddRange(activeNoneTag, activeSeriesTag, inactiveTag, commerceTag);
        context.ArtworkTags.AddRange(
            new ArtworkTag { Artwork = activeArtwork, Tag = activeNoneTag },
            new ArtworkTag { Artwork = activeArtwork, Tag = activeSeriesTag },
            new ArtworkTag { Artwork = activeArtwork, Tag = commerceTag },
            new ArtworkTag { Artwork = inactiveArtwork, Tag = inactiveTag });

        await context.SaveChangesAsync();

        var result = await service.GetTagsForActiveArtworksAsync();

        Assert.Collection(result,
            tag => Assert.Equal("A None Tag", tag.Name),
            tag => Assert.Equal("B Series Tag", tag.Name));
    }

    [Fact]
    public async Task ReturnsEmpty_WhenNoActiveArtworkTagsExist()
    {
        await using var context = CreateContext("GetTagsForActiveArtworksAsync_ReturnsEmpty_WhenNoActiveArtworkTagsExist");
        var service = new TagService(context);

        var artwork = new Artwork
        {
            CreatorId = "creator-1",
            Name = "Inactive Art",
            UploadDate = DateTime.UtcNow,
            IsActive = false
        };

        var tag = new Tag { Name = "Tag 1", Type = TagType.None };

        context.Artworks.Add(artwork);
        context.Tags.Add(tag);
        context.ArtworkTags.Add(new ArtworkTag { Artwork = artwork, Tag = tag });

        await context.SaveChangesAsync();

        var result = await service.GetTagsForActiveArtworksAsync();

        Assert.Empty(result);
    }
}
