using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DummyApp.StorageService.Data;
using DummyApp.StorageService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DummyApp.StorageService.Infrastructure.Services;

public sealed class TagService : ITagService
{
    private readonly StorageDbContext _dbContext;

    public TagService(StorageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TagDto>> GetTagsAsync()
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .Select(tag => new TagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Type = tag.Type.ToString()
            })
            .ToListAsync();
    }
}
