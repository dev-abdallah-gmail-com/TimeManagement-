using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using System;
using System.Threading;

namespace TimeManagement.Client.Services
{
 public class CustomAuthStateProvider : AuthenticationStateProvider
 {
 private readonly IAuthService _authService;
 private CancellationTokenSource? _cts;

 public CustomAuthStateProvider(IAuthService authService)
 {
 _authService = authService;
 }

 public override async Task<AuthenticationState> GetAuthenticationStateAsync()
 {
 var token = await _authService.GetTokenAsync();
 if (string.IsNullOrEmpty(token))
 {
 var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
 return new AuthenticationState(anonymous);
 }

 var claims = ParseClaimsFromJwt(token);
 var identity = new ClaimsIdentity(claims, "Bearer");
 var user = new ClaimsPrincipal(identity);
 return new AuthenticationState(user);
 }

 public async Task NotifyUserAuthentication(string token)
 {
 await _authService.LoginAsync(token);

 // Cancel previous expiration task if any
 _cts?.Cancel();
 _cts = new CancellationTokenSource();

 // Schedule auto-logout based on token expiration
 try
 {
 var exp = GetExpirationFromToken(token);
 if (exp.HasValue)
 {
 var delay = exp.Value - DateTime.UtcNow;
 if (delay <= TimeSpan.Zero)
 {
 await NotifyUserLogout();
 return;
 }

 _ = Task.Run(async () =>
 {
 try
 {
 await Task.Delay(delay, _cts.Token);
 await NotifyUserLogout();
 }
 catch (TaskCanceledException)
 {
 // ignore
 }
 });
 }
 }
 catch
 {
 // ignore expiration scheduling if parsing fails
 }

 NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
 }

 public async Task NotifyUserLogout()
 {
 _cts?.Cancel();
 _cts = null;

 await _authService.LogoutAsync();
 NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
 }

 private static DateTime? GetExpirationFromToken(string jwt)
 {
 try
 {
 var payload = jwt.Split('.')[1];
 payload = payload.PadRight(payload.Length + (4 - payload.Length %4) %4, '=');
 payload = payload.Replace('-', '+').Replace('_', '/');
 var jsonBytes = Convert.FromBase64String(payload);
 using var doc = JsonDocument.Parse(jsonBytes);
 if (doc.RootElement.TryGetProperty("exp", out var exp))
 {
 var seconds = exp.GetInt64();
 var date = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
 return date;
 }
 }
 catch
 {
 // ignore
 }

 return null;
 }

 private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
 {
 var claims = new List<Claim>();

 try
 {
 var payload = jwt.Split('.')[1];
 payload = payload.PadRight(payload.Length + (4 - payload.Length %4) %4, '=');
 payload = payload.Replace('-', '+').Replace('_', '/');
 var jsonBytes = Convert.FromBase64String(payload);

 using var doc = JsonDocument.Parse(jsonBytes);
 foreach (var property in doc.RootElement.EnumerateObject())
 {
 var name = property.Name;
 var value = property.Value;

 if (name.Equals("role", StringComparison.OrdinalIgnoreCase) || name.Equals("roles", StringComparison.OrdinalIgnoreCase))
 {
 if (value.ValueKind == JsonValueKind.String)
 {
 claims.Add(new Claim(ClaimTypes.Role, value.GetString() ?? string.Empty));
 }
 else if (value.ValueKind == JsonValueKind.Array)
 {
 foreach (var v in value.EnumerateArray())
 {
 claims.Add(new Claim(ClaimTypes.Role, v.GetString() ?? string.Empty));
 }
 }
 }
 else if (name.Equals("unique_name", StringComparison.OrdinalIgnoreCase) || name.Equals("name", StringComparison.OrdinalIgnoreCase))
 {
 claims.Add(new Claim(ClaimTypes.Name, value.GetString() ?? string.Empty));
 }
 else if (name.Equals("sub", StringComparison.OrdinalIgnoreCase))
 {
 claims.Add(new Claim(ClaimTypes.NameIdentifier, value.GetString() ?? string.Empty));
 }
 else if (name.Equals("email", StringComparison.OrdinalIgnoreCase))
 {
 claims.Add(new Claim(ClaimTypes.Email, value.GetString() ?? string.Empty));
 }
 else
 {
 // add other claims as custom
 if (value.ValueKind == JsonValueKind.String)
 {
 claims.Add(new Claim(name, value.GetString() ?? string.Empty));
 }
 }
 }
 }
 catch
 {
 // ignore parse errors
 }

 return claims;
 }
 }
}
