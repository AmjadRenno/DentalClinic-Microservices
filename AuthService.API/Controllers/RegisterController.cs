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
                return BadRequest(new { message = "Missing fields." });
            }

            var (success, errorMessage) = await _users.RegisterUserAsync(req.FullName, req.Email, req.Password);

            if (!success)
                return BadRequest(new { message = errorMessage });

            return Ok(new { message = "User registered successfully" });
        }
    }

    public record RegisterRequest(string FullName, string Email, string Password);
}
