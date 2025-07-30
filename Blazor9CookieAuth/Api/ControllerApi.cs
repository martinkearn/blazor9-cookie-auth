using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blazor9CookieAuth.Api;

[ApiController]
[Route("api/[controller]")]
public class ControllerApi : ControllerBase
{
    // Access with: curl -i http://localhost:5269/api/controllerapi
    // By default, this will return HTTP/1.1 401 Unauthorized
    // To make an authenticated request, first get a cookie: curl -i -X POST http://localhost:5269/api/auth/login -H "Content-Type: application/json" -d '"foo"'
    // Then make the request with the --cookie parameter and the full "Set-Cookie" output of api/auth/login: curl -i http://localhost:5269/api/controllerapi --cookie "your Set-Cookie value here" 
    
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        return Ok("You are authenticated on ControllerApi");
    }
}