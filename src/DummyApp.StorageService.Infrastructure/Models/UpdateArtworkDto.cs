using System;

namespace DummyApp.StorageService.Infrastructure.Models;

public sealed record UpdateArtworkDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public DateTime? CreationDate { get; init; }
    public DateTime? UploadDate { get; init; }
    public string? ImgUrl { get; init; }
    public string? ThumbnailUrl { get; init; }
    public bool? IsActive { get; init; }
}
