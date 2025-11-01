using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using TimeManagement.Client;
using TimeManagement.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// Register JS-based AuthService and custom AuthenticationStateProvider
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// Register the API authorization message handler
builder.Services.AddTransient<ApiAuthorizationMessageHandler>();

// Configure HttpClient to use the API backend and attach the bearer token via the handler
// Ensure base address matches the API server URL (change if your API runs on a different port)
builder.Services.AddHttpClient("ApiClient", client =>
{
 client.BaseAddress = new Uri("https://localhost:7010");
})
.AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

// Provide a default HttpClient for convenience that uses the named client
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient"));

await builder.Build().RunAsync();
