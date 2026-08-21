using DummyApp.StorageService.Infrastructure.Services;
using DummyApp.StorageService.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class VerificationController : ControllerBase
{
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly ICompletedOrdersService _completedOrdersService;
    private readonly ILogger<VerificationController> _logger;

    public VerificationController(
        IVerificationCodeService verificationCodeService,
        ICompletedOrdersService completedOrdersService,
        ILogger<VerificationController> logger)
    {
        _verificationCodeService = verificationCodeService;
        _completedOrdersService = completedOrdersService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVerificationCode([FromBody] CreateVerificationCodeRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Email, code and expiration are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!normalizedEmail.Contains('@') || request.Code.Length != 6)
        {
            return BadRequest("A valid email and 6-digit code are required.");
        }

        var created = await _verificationCodeService.CreateVerificationCodeAsync(normalizedEmail, request.Code.Trim(), request.ExpiresAt);
        if (!created)
        {
            _logger.LogError("Failed to create verification code record for email {Email}.", normalizedEmail);
            return BadRequest("Unable to create verification code.");
        }

        return Ok();
    }

    [HttpPost("completed-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCompletedOrdersToken([FromBody] CreateCompletedOrdersTokenRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || request.Token == Guid.Empty)
        {
            return BadRequest("Email and token are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!normalizedEmail.Contains('@') || request.ExpiresAt <= DateTime.UtcNow)
        {
            return BadRequest("A valid email and future expiration are required.");
        }

        var created = await _completedOrdersService.CreateCompletedOrdersTokenAsync(normalizedEmail, request.Token, request.ExpiresAt);
        if (!created)
        {
            _logger.LogError("Failed to create completed orders token for email {Email}.", normalizedEmail);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to persist completed orders token.");
        }

        return Ok();
    }

    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyVerificationCode([FromBody] VerifyVerificationCodeRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Email and code are required.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var normalizedCode = request.Code.Trim();
        if (!normalizedEmail.Contains('@') || normalizedCode.Length != 6)
        {
            return BadRequest("A valid email and 6-digit code are required.");
        }

        var verified = await _verificationCodeService.VerifyVerificationCodeAsync(normalizedEmail, normalizedCode);
        if (!verified)
        {
            return BadRequest("Invalid or expired verification code.");
        }

        return Ok();
    }
}
