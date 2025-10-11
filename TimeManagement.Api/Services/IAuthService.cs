using TimeManagement.Api.DTOs;

namespace TimeManagement.Api.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto, string role = "User");
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
}
