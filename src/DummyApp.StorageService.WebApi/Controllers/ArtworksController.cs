using System.Linq;
using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Models;
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
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArtworkDto>> CreateArtwork([FromBody] CreateArtworkRequest request)
    {
        if (request is null)
        {
            return BadRequest("Artwork request is required.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var totalTagCount = (request.ExistingTagIds?.Count() ?? 0) + (request.NewTags?.Count() ?? 0);
        if (totalTagCount > 10)
        {
            return BadRequest("A maximum of 10 tags is allowed.");
        }

        var artwork = await _artworkService.CreateArtworkAsync(request);
        if (artwork is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create artwork.");
        }

        return CreatedAtAction(nameof(GetArtworkById), new { id = artwork.Id }, artwork);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ArtworkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArtworkDto>>> GetArtworks([FromQuery] string? creatorId, [FromQuery] bool? isActive)
    {
        var artworks = await _artworkService.GetArtworksAsync(creatorId, isActive);
        return Ok(artworks ?? Array.Empty<ArtworkDto>());
    }

    [HttpGet("page")]
    [ProducesResponseType(typeof(PaginatedResult<ArtworkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<ArtworkDto>>> GetArtworksPage([FromQuery] string? creatorId, [FromQuery] bool? isActive, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _artworkService.GetArtworksPageAsync(creatorId, isActive, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtworkDto>> GetArtworkById([FromRoute] Guid id, [FromQuery] bool activeOnly = true)
    {
        var artwork = await _artworkService.GetArtworkByIdAsync(id, activeOnly);
        if (artwork == null)
        {
            return NotFound();
        }

        return Ok(artwork);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArtworkDto>> UpdateArtwork([FromRoute] Guid id, [FromBody] UpdateArtworkDto request)
    {
        if (request is null)
        {
            return BadRequest("Artwork request is required.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingArtwork = await _artworkService.GetArtworkByIdAsync(id, activeOnly: false);
        if (existingArtwork is null)
        {
            return NotFound();
        }

        var updatedArtwork = await _artworkService.UpdateArtworkAsync(id, request);
        if (updatedArtwork is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to update artwork.");
        }

        return Ok(updatedArtwork);
    }

    [HttpPut("{id}/active")]
    [ProducesResponseType(typeof(ArtworkDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArtworkDto>> UpdateArtworkIsActive([FromRoute] Guid id, [FromBody] UpdateArtworkIsActiveRequest request)
    {
        if (request is null)
        {
            return BadRequest("Artwork request is required.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingArtwork = await _artworkService.GetArtworkByIdAsync(id, activeOnly: false);
        if (existingArtwork is null)
        {
            return NotFound();
        }

        var updatedArtwork = await _artworkService.UpdateArtworkIsActiveAsync(id, request.IsActive.Value);
        if (updatedArtwork is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to update artwork active state.");
        }

        return Ok(updatedArtwork);
    }
}
