using System;
using System.Security.Claims;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DummyApp.StorageService.Infrastructure.Authorization;

public sealed class EditArtworkRequirement : IAuthorizationRequirement
{
}

public sealed class EditArtworkAuthorizationHandler : AuthorizationHandler<EditArtworkRequirement>
{
    private readonly IArtworkService _artworkService;

    public EditArtworkAuthorizationHandler(IArtworkService artworkService)
    {
        _artworkService = artworkService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        EditArtworkRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (context.Resource is not AuthorizationFilterContext filterContext)
        {
            return;
        }

        if (!filterContext.RouteData.Values.TryGetValue("id", out var idValue))
        {
            return;
        }

        var idString = idValue switch
        {
            int intId => intId.ToString(),
            string stringId => stringId,
            _ => idValue?.ToString() ?? string.Empty
        };

        if (!int.TryParse(idString, out var artworkId))
        {
            return;
        }

        var artwork = await _artworkService.GetArtworkByIdAsync(artworkId, activeOnly: false);
        if (artwork is null)
        {
            return;
        }

        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.User.IsInRole("Creator"))
        {
            var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(currentUserId, artwork.CreatorId, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }
    }
}
