using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet("testA")]
        [AllowAnonymous]
        public IActionResult GetAnonymous() => Ok(new { message = "StorageService API testA OK" });

        [HttpGet("testR")]
        [Authorize(Policy = "RequireStorageRead")]
        public IActionResult GetRead() => Ok(new { message = "StorageService API testR OK" });

        [HttpGet("testW")]
        [Authorize(Policy = "RequireStorageWrite")]
        public IActionResult GetWrite() => Ok(new { message = "StorageService API testW OK" });

        [HttpGet("testX")]
        [Authorize]
        public IActionResult GetConditional()
        {
            if (HasScope("storage.write"))
            {
                return Ok(new { message = "StorageService API testX: write access granted" });
            }

            if (HasScope("storage.read"))
            {
                return Ok(new { message = "StorageService API testX: read-only access granted" });
            }

            return Forbid();
        }

        private bool HasScope(string scope)
        {
            return User.Claims
                .Where(claim => claim.Type == "scope" || claim.Type == "scp")
                .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Any(value => value == scope);
        }
    }
}
