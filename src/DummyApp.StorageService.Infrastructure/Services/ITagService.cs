using DummyApp.StorageService.Infrastructure.Models;

namespace DummyApp.StorageService.Infrastructure.Services;

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetTagsAsync();
}
