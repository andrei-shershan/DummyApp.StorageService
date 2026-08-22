using System;

namespace DummyApp.StorageService.Data.Models;

public sealed class ArtworkTag
{
    public Guid ArtworkId { get; set; }
    public Guid TagId { get; set; }

    public Artwork? Artwork { get; set; }
    public Tag? Tag { get; set; }
}
