using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DummyApp.StorageService.WebApi.Models;
using DummyApp.StorageService.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/artworks")]
public class ArtworksController : ControllerBase
{
    private readonly IArtworkService _artworkService;

    public ArtworksController(IArtworkService artworkService)
    {
        _artworkService = artworkService;
    }

    [HttpPost]
    [Authorize(Policy = "CreateArtwork")]
    public async Task<IActionResult> CreateArtwork([FromBody] CreateArtworkRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creatorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(creatorId))
        {
            return Forbid();
        }

        var artwork = await _artworkService.CreateArtworkAsync(creatorId, request);

        return CreatedAtAction(nameof(GetArtworkById), new { id = artwork.Id }, artwork);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetArtworkById(int id)
    {
        var artwork = await _artworkService.GetArtworkByIdAsync(id);
        if (artwork == null)
        {
            return NotFound();
        }

        return Ok(artwork);
    }
}
