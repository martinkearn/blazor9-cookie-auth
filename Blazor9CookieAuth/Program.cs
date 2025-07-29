using System.Security.Claims;
using Blazor9CookieAuth.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Register cookie auth scheme
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options => {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
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
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor9CookieAuth.Client._Imports).Assembly);

app.MapPost("/api/auth/login", async (HttpContext ctx, IConfiguration config, [FromBody] string secret) =>
{
    var allowedSecrets = config.GetSection("AdminSecrets").Get<List<string>>();

    if (allowedSecrets is null || !allowedSecrets.Contains(secret))
        return Results.Unauthorized();

    var claims = new[] { new Claim(ClaimTypes.Name, "AdminUser") };
    var identity = new ClaimsIdentity(claims, "AdminCookie");
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("AdminCookie", principal);
    return Results.Ok();
});

app.Run();
