using System.Linq;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Mappings;
using DummyApp.StorageService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class ArtworkService : IArtworkService
{
    private readonly StorageDbContext _dbContext;
    private readonly ILogger<ArtworkService> _logger;

    public ArtworkService(StorageDbContext dbContext, ILogger<ArtworkService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ArtworkDto?> CreateArtworkAsync(ArtworkDto request)
    {
        if (request is null)
        {
            _logger.LogError("Artwork create request is null.");
            return null;
        }

        var artwork = request.ToEntity();
        _dbContext.Artworks.Add(artwork);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _dbContext.Entry(artwork).State = EntityState.Detached;
            _logger.LogError(ex, "Failed to save artwork to database.");
            return null;
        }
        catch (Exception ex)
        {
            _dbContext.Entry(artwork).State = EntityState.Detached;
            _logger.LogError(ex, "Unexpected error while creating artwork.");
            return null;
        }

        return artwork.ToDto();
    }

    public async Task<ArtworkDto?> GetArtworkByIdAsync(int id)
    {
        var artwork = await _dbContext.Artworks
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return artwork?.ToDto();
    }

    public async Task<IEnumerable<ArtworkDto>> GetArtworksByCreatorIdAsync(string creatorId)
    {
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            return Array.Empty<ArtworkDto>();
        }

        return await _dbContext.Artworks
            .AsNoTracking()
            .Where(a => a.CreatorId == creatorId)
            .Select(a => a.ToDto())
            .ToListAsync();
    }

    public async Task<IEnumerable<ArtworkDto>> GetAllArtworksAsync()
    {
        return await _dbContext.Artworks
            .AsNoTracking()
            .Select(a => a.ToDto())
            .ToListAsync();
    }
}
