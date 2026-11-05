using Auditor.Data;
using Auditor.Models;
using Auditor.Security;
using Auditor.Services;
using Auditor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region =========================
// LOGGING (Serilog)
#endregion

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/audit-log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog();

#region =========================
// SERVICES
#endregion

// MVC + Razor. Every page requires a signed-in user unless marked [AllowAnonymous].
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()));

    // The scaffolded entities use non-nullable strings for optional columns; without this every
    // such property (and navigation property) becomes implicitly [Required] and forms never save.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddRazorPages();

// DB Context
builder.Services.AddDbContext<AuditorContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AudConnection")
        ?? throw new InvalidOperationException("Connection string 'AudConnection' not found.")));

// Sign-in cookies survive container restarts when the key folder is on a volume.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrEmpty(keysPath))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("Auditor")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}

// Authentication (Cookie-based)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "Auditor.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Uploads (audit photos/attachments) up to 25 MB per request.
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o => o.MultipartBodyLengthLimit = 25 * 1024 * 1024);

// Application Services
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddSingleton<AttachmentStorage>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserScope>();
builder.Services.AddScoped<AuditFilterOptions>();
builder.Services.AddScoped<ResponsibilitySnapshot>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IAuditTemplateService, AuditTemplateService>();
builder.Services.AddScoped<IAppUserService, AppUserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IQuestionBankService, QuestionBankService>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddHostedService<ScheduleMonitoringHostedService>();

#region =========================
// BUILD APP
#endregion

var app = builder.Build();

#region =========================
// PIPELINE CONFIGURATION
#endregion

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}
else
{
    app.UseDeveloperExceptionPage();
}

// Friendly page for 404s and other empty error responses.
app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");

// HTTPS is terminated by the reverse proxy in front of the container; only redirect when the app itself serves HTTPS.
if (builder.Configuration.GetValue("Https:Redirect", false))
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

#region =========================
// ROUTING
#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

// Liveness probe for Docker / load balancers.
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

#region =========================
// DATABASE SEED
// Schema changes are applied by the auditor-migrate container (EF migrations bundle);
// the app only seeds reference data and the first administrator.
#endregion

if (builder.Configuration.GetValue("Seed:Enabled", true))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

#region =========================
// RUN
#endregion

try
{
    Log.Information("Starting Auditor application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
