using System;

namespace DummyApp.StorageService.Infrastructure.Models;

public sealed record SeriesDto
{
    public Guid Id { get; init; }
    public string CreatorId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
