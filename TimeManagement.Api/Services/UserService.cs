using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Models;

namespace TimeManagement.Api.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            });
        }

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = user.CreatedAt,
            Roles = roles.ToList()
        };
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
}
