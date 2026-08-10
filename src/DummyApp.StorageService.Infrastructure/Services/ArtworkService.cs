using System.Linq;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.Infrastructure.Authorization;
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

        if (!string.IsNullOrWhiteSpace(request.SeriesName))
        {
            var series = await GetOrCreateSeriesAsync(request.CreatorId, request.SeriesName);
            if (series is null)
            {
                _logger.LogError("Failed to resolve series {SeriesName} for creator {CreatorId}.", request.SeriesName, request.CreatorId);
                return null;
            }

            artwork.SeriesId = series.Id;
        }

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

    public async Task<IEnumerable<SeriesDto>> GetSeriesByCreatorAsync(string creatorId)
    {
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            return Array.Empty<SeriesDto>();
        }

        return await _dbContext.Series
            .AsNoTracking()
            .Where(s => s.CreatorId == creatorId)
            .OrderBy(s => s.Name)
            .Select(s => new SeriesDto
            {
                Id = s.Id,
                CreatorId = s.CreatorId,
                Name = s.Name
            })
            .ToListAsync();
    }

    public async Task<SeriesDto?> CreateSeriesAsync(string creatorId, string name)
    {
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            _logger.LogError("Series create request does not contain creator id.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogError("Series create request does not contain a valid name.");
            return null;
        }

        var normalizedName = name.Trim();
        var existingSeries = await _dbContext.Series
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.CreatorId == creatorId && s.Name == normalizedName);

        if (existingSeries is not null)
        {
            return new SeriesDto
            {
                Id = existingSeries.Id,
                CreatorId = existingSeries.CreatorId,
                Name = existingSeries.Name
            };
        }

        var series = new Series
        {
            CreatorId = creatorId,
            Name = normalizedName
        };

        _dbContext.Series.Add(series);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _dbContext.Entry(series).State = EntityState.Detached;
            _logger.LogError(ex, "Failed to save series to database.");
            return null;
        }
        catch (Exception ex)
        {
            _dbContext.Entry(series).State = EntityState.Detached;
            _logger.LogError(ex, "Unexpected error while creating series.");
            return null;
        }

        return new SeriesDto
        {
            Id = series.Id,
            CreatorId = series.CreatorId,
            Name = series.Name
        };
    }

    private async Task<SeriesDto?> GetOrCreateSeriesAsync(string creatorId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var normalizedName = name.Trim();
        var existingSeries = await _dbContext.Series
            .FirstOrDefaultAsync(s => s.CreatorId == creatorId && s.Name == normalizedName);

        if (existingSeries is not null)
        {
            return new SeriesDto
            {
                Id = existingSeries.Id,
                CreatorId = existingSeries.CreatorId,
                Name = existingSeries.Name
            };
        }

        var series = new Series
        {
            CreatorId = creatorId,
            Name = normalizedName
        };

        _dbContext.Series.Add(series);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _dbContext.Entry(series).State = EntityState.Detached;
            _logger.LogError(ex, "Failed to save series to database.");
            return null;
        }
        catch (Exception ex)
        {
            _dbContext.Entry(series).State = EntityState.Detached;
            _logger.LogError(ex, "Unexpected error while creating series.");
            return null;
        }

        return new SeriesDto
        {
            Id = series.Id,
            CreatorId = series.CreatorId,
            Name = series.Name
        };
    }

    public async Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly = true)
    {
        var artwork = await _dbContext.Artworks
            .AsNoTracking()
            .Include(a => a.Series)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (artwork is null)
        {
            return null;
        }

        if (activeOnly && !artwork.IsActive)
        {
            return null;
        }

        return artwork.ToDto();
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
        IQueryable<Artwork> query = _dbContext.Artworks
            .AsNoTracking()
            .Include(a => a.Series);

        if (!string.IsNullOrWhiteSpace(creatorId))
        {
            query = query.Where(a => a.CreatorId == creatorId);
        }

        if (isActive.HasValue && isActive.Value)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        return await query
            .Select(a => a.ToDto())
            .ToListAsync();
    }
}
