using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DummyApp.StorageService.Infrastructure.Models;

public sealed class CreateArtworkRequest
{
    [Required]
    [StringLength(100, ErrorMessage = "Name must be 100 characters or fewer.")]
    public string Name { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    [Required]
    [StringLength(1000, ErrorMessage = "Description must be 1000 characters or fewer.")]
    public string Description { get; init; } = string.Empty;

    [Required]
    public DateTime CreationDate { get; init; }

    [Required]
    public DateTime UploadDate { get; init; }

    [Required]
    public string ImgUrl { get; init; } = string.Empty;

    [Required]
    public string ThumbnailUrl { get; init; } = string.Empty;

    [Required]
    public bool IsActive { get; init; }

    [Required]
    public string CreatorId { get; init; } = string.Empty;

    public IEnumerable<Guid>? ExistingTagIds { get; init; }
    public IEnumerable<NewTagRequest>? NewTags { get; init; }
}
