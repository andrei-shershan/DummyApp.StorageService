using System.Collections.Generic;
using System.Threading.Tasks;
using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface IArtworkService
{
    Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto request);
    Task<ArtworkDto?> GetArtworkByIdAsync(int id);
    Task<IEnumerable<ArtworkDto>> GetArtworksByCreatorIdAsync(string creatorId);
    Task<IEnumerable<ArtworkDto>> GetAllArtworksAsync();
}
