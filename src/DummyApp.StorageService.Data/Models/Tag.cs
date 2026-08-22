using System;
using System.Collections.Generic;

namespace DummyApp.StorageService.Data.Models;

public sealed class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TagType Type { get; set; } = TagType.None;

    public ICollection<ArtworkTag> ArtworkTags { get; set; } = new List<ArtworkTag>();
}
