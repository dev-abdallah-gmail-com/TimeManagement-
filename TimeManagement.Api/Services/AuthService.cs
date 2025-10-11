using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TimeManagement.Api.DTOs;
using TimeManagement.Api.Models;

namespace TimeManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto, string role = "User")
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return null;
        }

        var user = new ApplicationUser
        {
            Email = registerDto.Email,
            UserName = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            return null;
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        await _userManager.AddToRoleAsync(user, role);

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Roles = roles
        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return null;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return null;
        }

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            FirstName = user.FirstName ?? "",
            LastName = user.LastName ?? "",
            Roles = roles
        };
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim("FirstName", user.FirstName ?? ""),
            new Claim("LastName", user.LastName ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
