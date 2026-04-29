using System.Linq;
using DummyApp.StorageService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DummyApp.StorageService.WebApi.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly StorageDbContext _db;

        public TestController(StorageDbContext db)
        {
            _db = db;
        }
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
                var message = _db.Messages
                    .Where(m => m.MessageTypeId == 1)
                    .OrderBy(m => m.Id)
                    .Select(m => new { m.Id, m.Text, m.MessageTypeId })
                    .FirstOrDefault();

                if (message == null)
                {
                    return NotFound(new { error = "No message found with MessageTypeId = 1" });
                }

                return Ok(message);
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
