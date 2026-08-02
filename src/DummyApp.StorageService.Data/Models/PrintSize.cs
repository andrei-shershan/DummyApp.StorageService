using System.Collections.Generic;

namespace DummyApp.StorageService.Data.Models;

public sealed class PrintSize
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Price> Prices { get; set; } = new List<Price>();
}
