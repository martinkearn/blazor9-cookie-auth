using Microsoft.AspNetCore.Authorization;

namespace Blazor9CookieAuth.Api;

public static class MinimalApi
{
    public static void MapMinimalApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/minimal", [Authorize] () => Results.Ok("You are authenticated")); 
        
        // Access as curl -i http://localhost:5269/api/minimal,
        // By default, this will return HTTP/1.1 401 Unauthorized
    }
}