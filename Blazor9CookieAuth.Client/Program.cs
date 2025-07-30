using Blazor9CookieAuth.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddAuthorizationCore(); // This enables support for [AuthorizeView] and cascading AuthenticationState
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>(); // This registers the custom authentication state provider (CookieAuthStateProvider) with the DI container as the implementation of AuthenticationStateProvider. 

await builder.Build().RunAsync();
