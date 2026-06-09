using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.Data.Models;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DummyApp.StorageService.WebApi.Services;

public sealed class ArtworkService : IArtworkService
{
    private readonly StorageDbContext _dbContext;

    public ArtworkService(StorageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Artwork> CreateArtworkAsync(string creatorId, CreateArtworkRequest request)
    {
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            throw new ArgumentException("CreatorId is required.", nameof(creatorId));
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Artwork name is required.", nameof(request.Name));
        }

        var artwork = new Artwork
        {
            CreatorId = creatorId,
            Name = request.Name.Trim(),
            PublicName = request.PublicName?.Trim() ?? string.Empty,
            Description = request.Description?.Trim() ?? string.Empty,
            CreationDate = request.CreationDate,
            UploadDate = DateTime.UtcNow,
            ImgUrl = request.ImgUrl?.Trim() ?? string.Empty,
            SmallImgUrl = request.SmallImgUrl?.Trim() ?? string.Empty,
            IsActive = request.IsActive
        };

        _dbContext.Artworks.Add(artwork);
        await _dbContext.SaveChangesAsync();

        return artwork;
    }

    public async Task<Artwork?> GetArtworkByIdAsync(int id)
    {
        return await _dbContext.Artworks
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Artwork>> GetAllArtworksAsync()
    {
        return await _dbContext.Artworks
            .AsNoTracking()
            .ToListAsync();
    }
}
