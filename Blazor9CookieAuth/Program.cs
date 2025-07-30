using Blazor9CookieAuth.Components;
using Blazor9CookieAuth.Models;
using Blazor9CookieAuth.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// Register cookie auth scheme
builder.Services.AddAuthentication(Consts.AdminCookieName)
    .AddCookie(Consts.AdminCookieName,options => {
        options.LoginPath = "/login"; // This option tells the authentication middleware “If a user tries to access a resource that requires authentication and they are not signed in, redirect them to /login”. It only applies when using automatic redirects, such as [Authorize] on Razor pages
        options.LogoutPath = "/logout"; // As LoginPath but for log out
        options.Cookie.Name = Consts.AdminCookieName;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // This option controls whether the authentication cookie is sent only over HTTPS or also over HTTP. CookieSecurePolicy.Always means HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // This option tells browsers: "Only send this cookie in first-party requests — never in any cross-site request, even top-level navigation from another origin."
        options.SlidingExpiration = true; // This option controls whether the cookie’s expiration time is refreshed (slid forward) with each request made by the user. Every time the user makes a request before the cookie expires, the system resets the expiration timer; the user stays logged in as long as they are active.
        options.ExpireTimeSpan = TimeSpan.FromDays(365); // This option sets the authentication cookie to expire 365 days after login (or after each request if sliding expiration is enabled).
    });
builder.Services.AddAuthorization(); // This enables the authorization system in ASP.NET Core. Required to use [Authorize] attributes and <AuthorizeView> components.
builder.Services.AddCascadingAuthenticationState(); //This registers a service that enables authentication state to flow through your component tree in Blazor. It’s required for <CascadingAuthenticationState> and <AuthorizeView> to work.
builder.Services.AddHttpContextAccessor(); // This registers IHttpContextAccessor, which lets you access the current HttpContext in places where it’s not injected automatically (inside the AuthApi).
builder.Services.AddHttpClient(); // This registers the IHttpClientFactory service, which allows other services in your server project to use HttpClient via dependency injection. Although the server is not making http requests, InteractiveAuto means client pages are temporarily rendered on the server and so a IHttpClientFactory is needed as the DI container is composed on the server side.

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
app.UseAuthentication(); // This Enables the authentication middleware, which: Validates incoming cookies (like your AdminAuthCookie), Populates HttpContext.User with the authenticated principal and claims. Required for [Authorize], <AuthorizeView>, or HttpContext.User to reflect the login state
app.UseAuthorization(); // This Enables the authorization middleware, which: Evaluates [Authorize] attributes and applies access control based on roles, policies, or schemes. Depends on UseAuthentication() running before it. Required to enforce [Authorize] and similar logic.
app.MapAuthEndpoints(); // This maps the custom //api/auth endpoints from AuthApi.cs
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor9CookieAuth.Client._Imports).Assembly);

app.Run();
