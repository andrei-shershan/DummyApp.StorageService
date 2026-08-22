using System;

namespace DummyApp.StorageService.Infrastructure.Models;

public sealed record TagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}
