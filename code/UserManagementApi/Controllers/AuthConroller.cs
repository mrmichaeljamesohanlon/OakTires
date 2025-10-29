using Microsoft.AspNetCore.Mvc;
using UserManagementApi.DTO;
using UserManagementApi.Services;

namespace UserManagementApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var success = await _authService.RegisterAsync(dto);
            if (!success)
            {
                _logger.LogWarning("Registration failed for {Username}", dto.Username);
                return BadRequest("Username or email already exists.");
            }

            _logger.LogInformation("User registered: {Username}", dto.Username);
            return Ok("Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token == null)
            {
                _logger.LogWarning("Login failed for {Username}", dto.Username);
                return Unauthorized("Invalid credentials.");
            }

            _logger.LogInformation("User logged in: {Username}", dto.Username);
            return Ok(new { token });
        }
    }

}
