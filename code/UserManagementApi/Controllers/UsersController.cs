using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementApi.DTO;
using UserManagementApi.Data;
using UserManagementApi.Services;

namespace UserManagementApi.Controllers
{


    [ApiController]
    [Route("users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(UserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", id);
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto dto)
        {
            var success = await _userService.UpdateUserAsync(id, dto);
            if (!success)
            {
                _logger.LogWarning("Failed to update user: {UserId}", id);
                return NotFound();
            }

            _logger.LogInformation("User updated: {UserId}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                _logger.LogWarning("Failed to delete user: {UserId}", id);
                return NotFound();
            }

            _logger.LogInformation("User deleted: {UserId}", id);
            return NoContent();
        }
    }
}

