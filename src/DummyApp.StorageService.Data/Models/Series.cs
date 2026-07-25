using System;
using System.Collections.Generic;

namespace DummyApp.StorageService.Data.Models;

public sealed class Series
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CreatorId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
