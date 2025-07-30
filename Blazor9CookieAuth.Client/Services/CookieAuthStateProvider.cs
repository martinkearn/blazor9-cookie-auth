using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazor9CookieAuth.Client.Services;

public class CookieAuthStateProvider(HttpClient http) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var result = await http.GetAsync("/auth/whoami");

        if (result.IsSuccessStatusCode)
        {
            var name = await result.Content.ReadAsStringAsync(); // Or parse JSON claims
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, name), new Claim("IsAdmin", "true") }, "AdminCookie");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}