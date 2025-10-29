using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using UserManagementApi.Data;
using UserManagementApi.DTO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace UserManagementApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration config, IHttpClientFactory httpFactory, ILogger<AuthService> logger)
        {
            _context = context;
            _config = config;
            _httpFactory = httpFactory;
            _logger = logger;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Username == dto.Username || u.Email == dto.Email))
                return false;

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Fire-and-forget webhook notification (errors won't block login)
            _ = SendLoginEventAsync(user);

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendLoginEventAsync(User user)
        {
            var url = _config["Webhook:LoginEventUrl"];
            if (string.IsNullOrWhiteSpace(url))
                return;

            try
            {
                var client = _httpFactory.CreateClient();
                var payload = new
                {
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    lastLoginAt = user.LastLoginAt ?? DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login webhook returned {StatusCode} for {Url}", response.StatusCode, url);
                }
            }
            catch (Exception ex)
            {
                // Log but do not fail the login flow
                _logger.LogError(ex, "Failed to send login webhook to {Url}", url);
            }
        }
    }
}

