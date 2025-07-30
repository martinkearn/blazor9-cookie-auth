using Blazor9CookieAuth.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddAuthorizationCore(); // Enables support for [AuthorizeView] and cascading AuthenticationState
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

await builder.Build().RunAsync();
