using Auditor.Models;
using Auditor.Services;
using Auditor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DB Context
builder.Services.AddDbContext<AuditorContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("AudConnection")
        ?? throw new InvalidOperationException("Connection string 'AudConnection' not found.")
    ));

// Authentication (Cookie-based)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Application Services
builder.Services.AddScoped<IAuditTemplateService, AuditTemplateService>();
builder.Services.AddScoped<IAppUserService, AppUserService>();
builder.Services.AddScoped<IQuestionBankService, QuestionBankService>();
builder.Services.AddScoped<IAuditScheduleService, AuditScheduleService>();

#region =========================
// BUILD APP
#endregion

var app = builder.Build();

#region =========================
// PIPELINE CONFIGURATION
#endregion

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

#region =========================
// ROUTING
#endregion

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapRazorPages();

#region =========================
// DATABASE MIGRATION
#endregion

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuditorContext>();
    await db.Database.MigrateAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Database migration failed during startup");
    throw;
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