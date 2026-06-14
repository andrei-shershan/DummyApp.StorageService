using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArtworksController : ControllerBase
{
    private readonly IArtworkService _artworkService;

    public ArtworksController(IArtworkService artworkService)
    {
        _artworkService = artworkService;
    }

    [HttpPost]
    [Authorize(Policy = "CreateArtwork")]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArtworkDto>> CreateArtwork([FromBody] ArtworkDto request)
    {
        if (request is null)
        {
            return BadRequest("Artwork request is required.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var artwork = await _artworkService.CreateArtworkAsync(request);
        if (artwork is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create artwork.");
        }

        return CreatedAtAction(nameof(GetArtworkById), new { id = artwork.Id }, artwork);
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ArtworkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArtworkDto>>> GetArtworks()
    {
        var artworks = await _artworkService.GetAllArtworksAsync();
        return Ok(artworks ?? Array.Empty<ArtworkDto>());
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtworkDto>> GetArtworkById([FromRoute] int id)
    {
        var artwork = await _artworkService.GetArtworkByIdAsync(id);
        if (artwork == null)
        {
            return NotFound();
        }

        return Ok(artwork);
    }
}
