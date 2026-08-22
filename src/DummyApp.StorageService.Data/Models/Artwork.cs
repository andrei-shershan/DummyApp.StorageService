using System;

namespace DummyApp.StorageService.Data.Models;

public sealed class Artwork
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CreatorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public DateTime UploadDate { get; set; }
    public string ImgUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
