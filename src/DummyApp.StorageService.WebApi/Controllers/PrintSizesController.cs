using DummyApp.StorageService.Infrastructure.Models;
using DummyApp.StorageService.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class PrintSizesController : ControllerBase
{
    private readonly IPrintSizeService _printSizeService;

    public PrintSizesController(IPrintSizeService printSizeService)
    {
        _printSizeService = printSizeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PrintSizeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrintSizeDto>>> GetPrintSizes()
    {
        var printSizes = await _printSizeService.GetPrintSizesAsync();
        return Ok(printSizes);
    }
}
