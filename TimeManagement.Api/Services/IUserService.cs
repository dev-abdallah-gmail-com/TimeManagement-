using TimeManagement.Api.DTOs;

namespace TimeManagement.Api.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<bool> DeleteUserAsync(string userId);
}
