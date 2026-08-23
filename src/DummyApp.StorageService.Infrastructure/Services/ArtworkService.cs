using System.Linq;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
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

    public async Task<ArtworkDto?> CreateArtworkAsync(CreateArtworkRequest request)
    {
        if (request is null)
        {
            _logger.LogError("Artwork create request is null.");
            return null;
        }

        var existingTagIds = request.ExistingTagIds?.ToArray() ?? Array.Empty<Guid>();

        var existingDbTags = new List<Tag>();
        if (existingTagIds.Length > 0)
        {
            foreach (var tagId in existingTagIds)
            {
                var tag = await _dbContext.Tags.FindAsync(tagId);
                if (tag is not null)
                {
                    existingDbTags.Add(tag);
                }
            }
        }

        var newDbTags = request.NewTags?.Any() == true ? request.NewTags
            .Select(t => new Tag
            {
                Id = Guid.NewGuid(),
                Name = t.Name.Trim(),
                Type = Enum.Parse<TagType>(t.Type, true)
            })
            .ToList() : new List<Tag>();

        var artwork = request.ToEntity();

        _dbContext.Artworks.Add(artwork);

        foreach (var existingDbTag in existingDbTags)
        {
            _dbContext.ArtworkTags.Add(new ArtworkTag
            {
                Artwork = artwork,
                Tag = existingDbTag
            });
        }

        foreach (var newDbTag in newDbTags)
        {
            _dbContext.Tags.Add(newDbTag);
            _dbContext.ArtworkTags.Add(new ArtworkTag
            {
                Artwork = artwork,
                Tag = newDbTag
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating artwork with tags.");
            return null;
        }

        return artwork.ToDto();
    }

    public async Task<ArtworkDto?> GetArtworkByIdAsync(Guid id, bool activeOnly = true)
    {
        var artwork = await _dbContext.Artworks
            .AsNoTracking()
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
            .AsNoTracking();

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

    public async Task<PaginatedResult<ArtworkDto>> GetArtworksPageAsync(string? creatorId = null, bool? isActive = null, int pageNumber = 1, int pageSize = 10, IEnumerable<Guid>? tagIds = null)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 10;
        }

        var filteredTagIds = tagIds?.Where(id => id != Guid.Empty).Distinct().ToArray() ?? Array.Empty<Guid>();

        IQueryable<Artwork> query = _dbContext.Artworks
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(creatorId))
        {
            query = query.Where(a => a.CreatorId == creatorId);
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        if (filteredTagIds.Any())
        {
            foreach (var tagId in filteredTagIds)
            {
                var tid = tagId;
                query = query.Where(a =>
                    _dbContext.ArtworkTags.Any(at => at.ArtworkId == a.Id && at.TagId == tid));
            }
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(a => a.UploadDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => a.ToDto())
            .ToListAsync();

        return new PaginatedResult<ArtworkDto>(items, pageNumber, pageSize, totalCount);
    }
}
