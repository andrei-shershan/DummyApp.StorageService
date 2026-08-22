using System.Linq;
using System.Linq.Expressions;
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

        var existingTagIds = request.ExistingTagIds?.Where(id => id != Guid.Empty).Distinct().ToArray() ?? Array.Empty<Guid>();
        var newTags = request.NewTags?.Where(tag => !string.IsNullOrWhiteSpace(tag.Name) && !string.IsNullOrWhiteSpace(tag.Type)).ToArray() ?? Array.Empty<NewTagRequest>();

        if (existingTagIds.Length + newTags.Length > 10)
        {
            _logger.LogWarning("Artwork create request contains more than 10 tags.");
            return null;
        }

        var existingTags = existingTagIds.Length > 0
            ? await _dbContext.Tags.Where(BuildTagIdPredicate(existingTagIds)).ToListAsync()
            : new List<Tag>();

        if (existingTagIds.Length != existingTags.Count)
        {
            _logger.LogWarning("Artwork create request contains invalid existing tag ids.");
            return null;
        }

        var normalizedNewTags = newTags
            .Select(tag => new { Name = tag.Name.Trim(), Type = tag.Type.Trim() })
            .ToArray();

        if (normalizedNewTags.GroupBy(tag => $"{tag.Name}|{tag.Type}", StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            _logger.LogWarning("Artwork create request contains duplicate new tags.");
            return null;
        }

        var existingSeriesCount = existingTags.Count(t => t.Type == TagType.Series);
        var newSeriesCount = normalizedNewTags.Count(tag => tag.Type.Equals(TagType.Series.ToString(), StringComparison.OrdinalIgnoreCase));
        if (existingSeriesCount + newSeriesCount > 1)
        {
            _logger.LogWarning("Artwork create request contains more than one series tag.");
            return null;
        }

        var newTagNames = normalizedNewTags
            .Select(tag => tag.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var matchingExistingTags = newTagNames.Length > 0
            ? await _dbContext.Tags
                .Where(BuildTagNamePredicate(newTagNames))
                .ToListAsync()
            : new List<Tag>();

        foreach (var newTag in normalizedNewTags)
        {
            if (matchingExistingTags.Any(tag => tag.Type.ToString().Equals(newTag.Type, StringComparison.OrdinalIgnoreCase)
                && tag.Name.Equals(newTag.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Artwork create request contains a new tag that already exists: {TagName} ({TagType}).", newTag.Name, newTag.Type);
                return null;
            }
        }

        var parsedNewTags = new List<Tag>();
        foreach (var newTag in normalizedNewTags)
        {
            if (!Enum.TryParse<TagType>(newTag.Type, true, out var parsedType) || (parsedType != TagType.None && parsedType != TagType.Series))
            {
                _logger.LogWarning("Artwork create request contains an invalid tag type: {TagType}.", newTag.Type);
                return null;
            }

            parsedNewTags.Add(new Tag
            {
                Name = newTag.Name,
                Type = parsedType
            });
        }

        var artwork = request.ToEntity();
        _dbContext.Artworks.Add(artwork);

        foreach (var tag in existingTags)
        {
            _dbContext.ArtworkTags.Add(new ArtworkTag
            {
                Artwork = artwork,
                Tag = tag
            });
        }

        foreach (var tag in parsedNewTags)
        {
            _dbContext.Tags.Add(tag);
            _dbContext.ArtworkTags.Add(new ArtworkTag
            {
                Artwork = artwork,
                Tag = tag
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _dbContext.Entry(artwork).State = EntityState.Detached;
            foreach (var tag in parsedNewTags)
            {
                _dbContext.Entry(tag).State = EntityState.Detached;
            }

            _logger.LogError(ex, "Failed to save artwork with tags to database.");
            return null;
        }
        catch (Exception ex)
        {
            _dbContext.Entry(artwork).State = EntityState.Detached;
            foreach (var tag in parsedNewTags)
            {
                _dbContext.Entry(tag).State = EntityState.Detached;
            }

            _logger.LogError(ex, "Unexpected error while creating artwork with tags.");
            return null;
        }

        return artwork.ToDto();
    }

    private static Expression<Func<Tag, bool>> BuildTagIdPredicate(IEnumerable<Guid> tagIds)
    {
        var ids = tagIds.ToArray();
        if (ids.Length == 0)
        {
            return tag => false;
        }

        var parameter = Expression.Parameter(typeof(Tag), "tag");
        var property = Expression.Property(parameter, nameof(Tag.Id));
        Expression body = Expression.Equal(property, Expression.Constant(ids[0], typeof(Guid)));

        for (var i = 1; i < ids.Length; i++)
        {
            body = Expression.OrElse(body,
                Expression.Equal(property, Expression.Constant(ids[i], typeof(Guid))));
        }

        var idPredicate = Expression.Lambda<Func<Tag, bool>>(body, parameter);
        return idPredicate;
    }

    private static Expression<Func<Tag, bool>> BuildTagNamePredicate(IEnumerable<string> tagNames)
    {
        var names = tagNames.ToArray();
        if (names.Length == 0)
        {
            return tag => false;
        }

        var parameter = Expression.Parameter(typeof(Tag), "tag");
        var property = Expression.Property(parameter, nameof(Tag.Name));
        Expression body = Expression.Equal(property, Expression.Constant(names[0], typeof(string)));

        for (var i = 1; i < names.Length; i++)
        {
            body = Expression.OrElse(body,
                Expression.Equal(property, Expression.Constant(names[i], typeof(string))));
        }

        return Expression.Lambda<Func<Tag, bool>>(body, parameter);
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
}
