using System.Threading.Tasks;

namespace TimeManagement.Client.Services
{
 public interface IAuthService
 {
 Task LoginAsync(string token);
 Task LogoutAsync();
 Task<string?> GetTokenAsync();
 Task<bool> IsLoggedInAsync();
 }
}
