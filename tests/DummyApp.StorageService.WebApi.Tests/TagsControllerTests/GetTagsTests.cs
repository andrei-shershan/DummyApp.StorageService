using System;
using System.Collections.Generic;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DummyApp.StorageService.WebApi.Tests.TagsControllerTests;

public sealed class GetTagsTests
{
    [Fact]
    public async Task GetTags_ReturnsOkWithTagList()
    {
        var expected = new[]
        {
            new TagDto { Id = Guid.NewGuid(), Name = "Tag 1", Type = "TypeA" },
            new TagDto { Id = Guid.NewGuid(), Name = "Tag 2", Type = "TypeB" }
        };

        var tagService = new Mock<ITagService>();
        tagService.Setup(x => x.GetTagsAsync()).ReturnsAsync(expected);

        var controller = new TagsController(tagService.Object);

        var result = await controller.GetTags();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsAssignableFrom<IEnumerable<TagDto>>(okResult.Value);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetTags_ReturnsOkWithEmptyList_WhenNoTags()
    {
        var tagService = new Mock<ITagService>();
        tagService.Setup(x => x.GetTagsAsync()).ReturnsAsync(Array.Empty<TagDto>());

        var controller = new TagsController(tagService.Object);

        var result = await controller.GetTags();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actual = Assert.IsAssignableFrom<IEnumerable<TagDto>>(okResult.Value);

        Assert.Empty(actual);
    }
}
