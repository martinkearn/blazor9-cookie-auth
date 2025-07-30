using System.Net;
using System.Security.Claims;
using Blazor9CookieAuth.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazor9CookieAuth.Client.Services;

public class CookieAuthStateProvider(HttpClient http) : AuthenticationStateProvider
{
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var result = await http.GetAsync("/api/auth/state");

        // If the result is anything other than OK (api returns NoContent if not authenticated), issue a blank AuthenticationState
        if (result.StatusCode != HttpStatusCode.OK) return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        
        // If the result is OK, create and return a ClaimsIdentity
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], Consts.AdminCookieName); // "Cookies" is the same as CookieAuthenticationDefaults.AuthenticationScheme
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}