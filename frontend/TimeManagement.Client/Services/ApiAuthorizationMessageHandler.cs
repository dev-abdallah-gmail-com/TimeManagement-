using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TimeManagement.Client.Services
{
 public class ApiAuthorizationMessageHandler : DelegatingHandler
 {
 private readonly IAuthService _authService;
 private readonly ILogger<ApiAuthorizationMessageHandler> _logger;

 public ApiAuthorizationMessageHandler(IAuthService authService, ILogger<ApiAuthorizationMessageHandler> logger)
 {
 _authService = authService;
 _logger = logger;
 }

 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
 {
 try
 {
 var token = await _authService.GetTokenAsync();
 if (!string.IsNullOrEmpty(token))
 {
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 }
 }
 catch (System.Exception ex)
 {
 _logger.LogError(ex, "Error getting token for request");
 }

 return await base.SendAsync(request, cancellationToken);
 }
 }
}
