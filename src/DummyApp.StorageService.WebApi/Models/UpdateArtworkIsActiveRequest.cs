using System.ComponentModel.DataAnnotations;

namespace DummyApp.StorageService.WebApi.Models;

public sealed class UpdateArtworkIsActiveRequest
{
    [Required]
    public bool? IsActive { get; init; }
}
