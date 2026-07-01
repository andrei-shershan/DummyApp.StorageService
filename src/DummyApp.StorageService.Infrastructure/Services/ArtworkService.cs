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

    public async Task<ArtworkDto?> GetArtworkByIdAsync(int id, bool activeOnly = true)
    {
        var query = _dbContext.Artworks.AsNoTracking().Where(a => a.Id == id);

        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        var artwork = await query.FirstOrDefaultAsync();
        return artwork?.ToDto();
    }

    public async Task<IEnumerable<ArtworkDto>> GetArtworksAsync(string? creatorId = null, bool? isActive = null)
    {
        var query = _dbContext.Artworks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(creatorId))
        {
            query = query.Where(a => a.CreatorId == creatorId);
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        return await query
            .Select(a => a.ToDto())
            .ToListAsync();
    }
}
