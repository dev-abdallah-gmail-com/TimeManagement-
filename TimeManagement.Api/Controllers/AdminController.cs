using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Services;

namespace TimeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserDto>> GetUserById(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }

    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        var result = await _userService.DeleteUserAsync(userId);

        if (!result)
        {
            return NotFound(new { message = "User not found or could not be deleted." });
        }

        return NoContent();
    }
}
