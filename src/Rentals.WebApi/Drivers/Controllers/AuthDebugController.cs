using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Rentals.WebApi.Auth.Controllers;

[ApiController]
[Route("auth")]
public class AuthDebugController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var claims = User.Claims
            .Select(c => new { c.Type, c.Value })
            .ToList();

        return Ok(new
        {
            User.Identity?.IsAuthenticated,
            User.Identity?.Name,
            Claims = claims
        });
    }
}