using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Mappings;

internal static class ArtworkMapper
{
    public static ArtworkDto ToDto(this Artwork artwork)
    {
        return new ArtworkDto
        {
            Id = artwork.Id,
            CreatorId = artwork.CreatorId,
            Name = artwork.Name,
            Description = artwork.Description,
            CreationDate = artwork.CreationDate,
            UploadDate = artwork.UploadDate,
            ImgUrl = artwork.ImgUrl,
            ThumbnailUrl = artwork.ThumbnailUrl,
            IsActive = artwork.IsActive
        };
    }

    public static Artwork ToEntity(this ArtworkDto request)
    {
        return new Artwork
        {
            CreatorId = request.CreatorId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            CreationDate = request.CreationDate,
            UploadDate = request.UploadDate == default ? DateTime.UtcNow : request.UploadDate,
            ImgUrl = request.ImgUrl?.Trim() ?? string.Empty,
            ThumbnailUrl = request.ThumbnailUrl?.Trim() ?? string.Empty,
            IsActive = request.IsActive
        };
    }
}
