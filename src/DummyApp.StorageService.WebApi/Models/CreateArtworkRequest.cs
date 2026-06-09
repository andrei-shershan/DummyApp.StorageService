using System;

namespace DummyApp.StorageService.WebApi.Models;

public sealed class CreateArtworkRequest
{
    public string CreatorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PublicName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public string ImgUrl { get; set; } = string.Empty;
    public string SmallImgUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
