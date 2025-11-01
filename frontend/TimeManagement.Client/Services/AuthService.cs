using System.Threading.Tasks;
using Microsoft.JSInterop;
using System;

namespace TimeManagement.Client.Services
{
 public class AuthService : IAuthService
 {
 private readonly IJSRuntime _jsRuntime;
 private const string TokenKey = "authToken";

 public AuthService(IJSRuntime jsRuntime)
 {
 _jsRuntime = jsRuntime;
 }

 public async Task LoginAsync(string token)
 {
 await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
 }

 public async Task LogoutAsync()
 {
 await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
 }

 public async Task<string?> GetTokenAsync()
 {
 return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
 }

 public async Task<bool> IsLoggedInAsync()
 {
 var token = await GetTokenAsync();
 if (string.IsNullOrEmpty(token)) return false;
 // Optionally check expiration
 try
 {
 var payload = token.Split('.')[1];
 payload = payload.PadRight(payload.Length + (4 - payload.Length %4) %4, '=');
 payload = payload.Replace('-', '+').Replace('_', '/');
 var jsonBytes = Convert.FromBase64String(payload);
 using var doc = System.Text.Json.JsonDocument.Parse(jsonBytes);
 if (doc.RootElement.TryGetProperty("exp", out var exp))
 {
 var seconds = exp.GetInt64();
 var date = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
 return date > DateTime.UtcNow;
 }
 }
 catch
 {
 }
 return true;
 }
 }
}
