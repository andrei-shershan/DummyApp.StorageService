using System.Collections.Generic;
using System.Threading.Tasks;
using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IArtworkService
{
    Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto request);
    Task<ArtworkDto?> UpdateArtworkAsync(Guid id, UpdateArtworkDto request);
    Task<ArtworkDto?> UpdateArtworkIsActiveAsync(Guid id, bool isActive);
    Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly = true);
    Task<IEnumerable<ArtworkDto>> GetArtworksAsync(string? creatorId = null, bool? isActive = null);
    Task<IEnumerable<SeriesDto>> GetSeriesByCreatorAsync(string creatorId);
    Task<SeriesDto?> CreateSeriesAsync(string creatorId, string name);
}
