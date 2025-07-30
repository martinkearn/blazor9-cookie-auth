using Microsoft.AspNetCore.Authorization;

namespace Blazor9CookieAuth.Api;

public static class MinimalApi
{
    public static void MapMinimalApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/minimalapi", [Authorize] () => Results.Ok("You are authenticated on MinimalApi")); 
        
        // Access with: curl -i http://localhost:5269/api/minimalapi,
        // By default, this will return HTTP/1.1 401 Unauthorized
        // To make an authenticated request, first get a cookie: curl -i -X POST http://localhost:5269/api/auth/login -H "Content-Type: application/json" -d '"foo"'
        // Then make the request with the --cookie parameter and the full "Set-Cookie" output of api/auth/login: curl -i http://localhost:5269/api/minimalapi --cookie "your Set-Cookie value here" 
    }
}