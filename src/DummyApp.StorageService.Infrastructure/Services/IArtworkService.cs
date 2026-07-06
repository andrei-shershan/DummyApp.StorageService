using System.Collections.Generic;
using System.Threading.Tasks;
using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IArtworkService
{
    Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto request);
    Task<ArtworkDto?> UpdateArtworkAsync(int id, UpdateArtworkDto request);
    Task<ArtworkDto?> UpdateArtworkIsActiveAsync(int id, bool isActive);
    Task<ArtworkDto?> GetArtworkByIdAsync(int id, bool activeOnly = true);
    Task<IEnumerable<ArtworkDto>> GetArtworksAsync(string? creatorId = null, bool? isActive = null);
}
