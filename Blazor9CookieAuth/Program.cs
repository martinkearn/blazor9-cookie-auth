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
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("LocalApi")
    .ConfigureHttpClient((sp, client) =>
    {
        var context = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
        if (context is null) return;
        var origin = $"{context.Request.Scheme}://{context.Request.Host}";
        client.BaseAddress = new Uri(origin);
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
app.MapAuthEndpoints(); // This maps the custom //api/auth endpoints from AuthApi.cs
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor9CookieAuth.Client._Imports).Assembly);

app.Run();
