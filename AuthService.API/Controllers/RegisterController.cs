using AuthService.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("auth")]
    public class RegisterController : ControllerBase
    {
        private readonly UserService _users;

        public RegisterController(UserService users)
        {
            _users = users;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Missing fields.");
            }

            var ok = await _users.RegisterUserAsync(req.FullName, req.Email, req.Password);

            if (!ok)
                return Conflict("Email already exists");

            return Ok(new { message = "User registered" });
        }
    }

    public record RegisterRequest(string FullName, string Email, string Password);
}
