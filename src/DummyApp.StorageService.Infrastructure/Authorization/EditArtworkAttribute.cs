using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.Infrastructure.Authorization;

public sealed class EditArtworkAttribute : TypeFilterAttribute
{
    public EditArtworkAttribute() : base(typeof(EditArtworkAuthorizationFilter))
    {
    }
}
