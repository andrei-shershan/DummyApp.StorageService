using System.Collections.Generic;
using System.Threading.Tasks;
using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IArtworkService
{
    Task<ArtworkDto?> CreateArtworkAsync(CreateArtworkRequest request);
    Task<ArtworkDto?> UpdateArtworkAsync(Guid id, UpdateArtworkDto request);
    Task<ArtworkDto?> UpdateArtworkIsActiveAsync(Guid id, bool isActive);
    Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly = true);
    Task<IEnumerable<ArtworkDto>> GetArtworksAsync(string? creatorId = null, bool? isActive = null);
    Task<PaginatedResult<ArtworkDto>> GetArtworksPageAsync(string? creatorId = null, bool? isActive = null, int pageNumber = 1, int pageSize = 10);
}
