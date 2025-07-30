using System.Security.Claims;
using Blazor9CookieAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Blazor9CookieAuth.Api;

public static class AuthApi
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/state", (HttpContext ctx) =>
        {
            var user = ctx.User;
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                return Results.Ok(new
                {
                    name = user.Identity.Name,
                    claims = user.Claims.Select(c => new { c.Type, c.Value })
                });
            }

            return Results.NoContent();
        });

        app.MapPost("/api/auth/login", async (HttpContext ctx, IConfiguration config, [FromBody] string secret) =>
        {
            var allowedSecrets = config.GetSection("AdminSecrets").Get<List<string>>();
            if (allowedSecrets is null) return Results.NotFound("No allowed secrets are set on the server");
            if (!allowedSecrets.Contains(secret)) return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, "Administrator"),
            };

            var claimsIdentity = new ClaimsIdentity(claims, Consts.AdminCookieName);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await ctx.SignInAsync(Consts.AdminCookieName, new ClaimsPrincipal(claimsIdentity), authProperties);
            return Results.Ok();
        });

        app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(Consts.AdminCookieName);
            return Results.Ok();
        });
    }
}