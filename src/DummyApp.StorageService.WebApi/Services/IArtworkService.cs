using System.Collections.Generic;
using System.Threading.Tasks;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.WebApi.Models;

namespace DummyApp.StorageService.WebApi.Services;

public interface IArtworkService
{
    Task<Artwork> CreateArtworkAsync(string creatorId, CreateArtworkRequest request);
    Task<Artwork?> GetArtworkByIdAsync(int id);
    Task<IReadOnlyList<Artwork>> GetAllArtworksAsync();
}
