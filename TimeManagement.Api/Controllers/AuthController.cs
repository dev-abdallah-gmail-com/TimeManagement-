using Microsoft.AspNetCore.Mvc;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Services;

namespace TimeManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);

        if (result == null)
        {
            return BadRequest(new { message = "User registration failed. Email may already exist." });
        }

        return Ok(result);
    }

    [HttpPost("register-admin")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAdmin([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto, "Admin");

        if (result == null)
        {
            return BadRequest(new { message = "Admin registration failed. Email may already exist." });
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);

        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return Ok(result);
    }
}
