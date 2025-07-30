using System.Security.Claims;
using Blazor9CookieAuth.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Register cookie auth scheme
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;

        options.Events.OnSigningIn = context =>
        {
            Console.WriteLine("Signing in: " + context.Principal.Identity?.Name);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("LocalApi")
    .ConfigureHttpClient((sp, client) =>
    {
        var context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
        if (context is not null)
        {
            var origin = $"{context.Request.Scheme}://{context.Request.Host}";
            client.BaseAddress = new Uri(origin);
        }
    });
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("LocalApi");
});
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor9CookieAuth.Client._Imports).Assembly);

app.MapGet("/auth/whoami", (HttpContext ctx) =>
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

    return Results.Unauthorized();
});

app.MapPost("/api/auth/login", async (HttpContext ctx, IConfiguration config, [FromBody] string secret) =>
{
    // Get a list of secrets from config
    var allowedSecrets = config.GetSection("AdminSecrets").Get<List<string>>();
    if (allowedSecrets is null) return Results.NotFound("No allowed secrets are set on the server");
    
    // Verify secret is in the allowed list, if not return unauthorised
    if (!allowedSecrets.Contains(secret)) return Results.Unauthorized();
    
    var claims = new List<Claim>
    {
        new(ClaimTypes.Role, "Administrator"),
    };
    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var authProperties = new AuthenticationProperties
    {
        IsPersistent = true
    };
    
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
    Console.WriteLine("SignInAsync complete");
    
    // Log all Set-Cookie headers
    if (ctx.Response.Headers.TryGetValue("Set-Cookie", out var setCookieHeaders))
    {
        foreach (var header in setCookieHeaders)
        {
            Console.WriteLine("Set-Cookie: " + header);
        }
    }
    else
    {
        Console.WriteLine("No Set-Cookie header found.");
    }
    
    //return Results.Ok();
    return Results.Json(new { success = true }, statusCode: 200);
});

app.MapPost("/api/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.Run();
