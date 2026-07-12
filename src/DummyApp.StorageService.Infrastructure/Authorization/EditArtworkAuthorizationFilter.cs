using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DummyApp.StorageService.Infrastructure.Authorization;

public sealed class EditArtworkAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IArtworkService _artworkService;

    public EditArtworkAuthorizationFilter(IAuthorizationService authorizationService, IArtworkService artworkService)
    {
        _authorizationService = authorizationService;
        _artworkService = artworkService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var routeData = context.RouteData.Values;
        if (!routeData.TryGetValue("id", out var idValue))
        {
            context.Result = new ForbidResult();
            return;
        }

        Guid artworkId;
        if (idValue is Guid idGuid)
        {
            artworkId = idGuid;
        }
        else if (idValue is string idString && Guid.TryParse(idString, out var parsedId))
        {
            artworkId = parsedId;
        }
        else
        {
            context.Result = new ForbidResult();
            return;
        }

        var artwork = await _artworkService.GetArtworkByIdAsync(artworkId, activeOnly: false);
        if (artwork is null)
        {
            context.Result = new NotFoundResult();
            return;
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(context.HttpContext.User, artwork, "EditArtwork");
        if (!authorizationResult.Succeeded)
        {
            context.Result = new ForbidResult();
        }
    }
}
