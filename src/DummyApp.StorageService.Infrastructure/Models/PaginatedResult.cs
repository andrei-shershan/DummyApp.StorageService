namespace DummyApp.StorageService.Infrastructure.Models;

public sealed record PaginatedResult<T>(IReadOnlyCollection<T> Items, int PageNumber, int PageSize, int TotalCount);