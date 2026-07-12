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

    public async Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly = true)
    {
        var query = _dbContext.Artworks.AsNoTracking().Where(a => a.Id == id);

        if (activeOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        var artwork = await query.FirstOrDefaultAsync();
        return artwork?.ToDto();
    }

    public async Task<ArtworkDto?> UpdateArtworkAsync(Guid id, UpdateArtworkDto request)
    {
        if (request is null)
        {
            _logger.LogError("Artwork update request is null.");
            return null;
        }

        var artwork = await _dbContext.Artworks.FindAsync(id);
        if (artwork is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            artwork.Name = request.Name.Trim();
        }

        if (request.Description is not null)
        {
            artwork.Description = request.Description.Trim();
        }

        if (request.CreationDate.HasValue)
        {
            artwork.CreationDate = request.CreationDate.Value;
        }

        if (request.UploadDate.HasValue)
        {
            artwork.UploadDate = request.UploadDate.Value;
        }

        if (request.ImgUrl is not null)
        {
            artwork.ImgUrl = request.ImgUrl.Trim();
        }

        if (request.ThumbnailUrl is not null)
        {
            artwork.ThumbnailUrl = request.ThumbnailUrl.Trim();
        }

        if (request.IsActive.HasValue)
        {
            artwork.IsActive = request.IsActive.Value;
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save updated artwork to database.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating artwork.");
            return null;
        }

        return artwork.ToDto();
    }

    public async Task<ArtworkDto?> UpdateArtworkIsActiveAsync(Guid id, bool isActive)
    {
        var artwork = await _dbContext.Artworks.FindAsync(id);
        if (artwork is null)
        {
            return null;
        }

        artwork.IsActive = isActive;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save artwork active state to database.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating artwork active state.");
            return null;
        }

        return artwork.ToDto();
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
