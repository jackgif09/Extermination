using Extermination.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase);

builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath         = "/Account/Login";
        o.AccessDeniedPath  = "/Account/Login";
        o.ExpireTimeSpan    = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.Name       = "CimeXAdmin";
        o.Cookie.HttpOnly   = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.Cookie.SameSite   = SameSiteMode.Strict;
    });

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// In production, the admin password must be explicitly configured.
// Set the environment variable: Admin__Password=<your-password>
if (!app.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(app.Configuration["Admin:Password"]))
        throw new InvalidOperationException(
            "Admin password is not configured. Set the 'Admin__Password' environment variable.");
}

// ── Middleware pipeline ────────────────────────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
