using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SeriesController : ControllerBase
{
    private readonly IArtworkService _artworkService;

    public SeriesController(IArtworkService artworkService)
    {
        _artworkService = artworkService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<SeriesDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SeriesDto>>> GetSeries([FromQuery] string creatorId)
    {
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            return BadRequest("creatorId is required.");
        }

        var series = await _artworkService.GetSeriesByCreatorAsync(creatorId);
        return Ok(series);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(SeriesDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SeriesDto>> CreateSeries([FromBody] CreateSeriesRequest request)
    {
        if (request is null)
        {
            return BadRequest("Series request is required.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var creatorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(creatorId))
        {
            return Forbid();
        }

        var series = await _artworkService.CreateSeriesAsync(creatorId, request.Name);
        if (series is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to create series.");
        }

        return CreatedAtAction(nameof(GetSeries), new { creatorId }, series);
    }
}
