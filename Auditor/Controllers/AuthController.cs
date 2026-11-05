using System.Security.Claims;
using Auditor.Models;
using Auditor.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuditorContext _context;
        private readonly PasswordService _passwords;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuditorContext context, PasswordService passwords, ILogger<AuthController> logger)
        {
            _context = context;
            _passwords = passwords;
            _logger = logger;
        }

        // GET: /login
        [AllowAnonymous]
        [HttpGet("/login")]
        [HttpGet("/Auth/Login")]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLocal(returnUrl);

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /login
        [AllowAnonymous]
        [HttpPost("/login")]
        [HttpPost("/Auth/Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string? login, string? password, bool rememberMe = false, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Login = login;

            login = login?.Trim();
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Enter your email or employee number, and your password.";
                return View();
            }

            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Email == login || u.EmployeeNumber == login);

            var check = user == null ? PasswordCheck.Failed : _passwords.Verify(user, password);

            if (user == null || check == PasswordCheck.Failed)
            {
                _logger.LogWarning("Failed sign-in for {Login}", login);
                ViewBag.Error = "Incorrect email/employee number or password.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Your account is inactive. Contact an administrator to reactivate it.";
                return View();
            }

            if (check == PasswordCheck.SuccessRehashNeeded)
                user.PasswordHash = _passwords.Hash(user, password);

            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SignInAsync(user, rememberMe);
            _logger.LogInformation("User {UserId} signed in", user.Id);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.Role == Roles.Auditor
                ? RedirectToAction("Index", "AuditExecutions")
                : RedirectToAction("Index", "Dashboard");
        }

        // POST: /logout
        [HttpPost("/logout")]
        [HttpPost("/Auth/Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }

        // GET: /account/password
        [HttpGet("/account/password")]
        public IActionResult ChangePassword() => View();

        // POST: /account/password
        [HttpPost("/account/password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string? currentPassword, string? newPassword, string? confirmPassword)
        {
            if (!long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return RedirectToAction(nameof(Login));

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
                return RedirectToAction(nameof(Login));

            if (string.IsNullOrEmpty(currentPassword) || _passwords.Verify(user, currentPassword) == PasswordCheck.Failed)
                ModelState.AddModelError(nameof(currentPassword), "Current password is incorrect.");

            var problem = PasswordService.Validate(newPassword);
            if (problem != null)
                ModelState.AddModelError(nameof(newPassword), problem);
            else if (newPassword != confirmPassword)
                ModelState.AddModelError(nameof(confirmPassword), "The new passwords don't match.");

            if (!ModelState.IsValid)
                return View();

            user.PasswordHash = _passwords.Hash(user, newPassword!);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your password was changed.";
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /access-denied
        [AllowAnonymous]
        [HttpGet("/access-denied")]
        public IActionResult AccessDenied() => View();

        private async Task SignInAsync(AppUser user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName ?? user.Email ?? $"User {user.Id}"),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, Roles.All.Contains(user.Role) ? user.Role : Roles.Viewer),
            };

            if (!string.IsNullOrEmpty(user.EmployeeNumber))
                claims.Add(new Claim("employee_number", user.EmployeeNumber));

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8),
                });
        }

        private IActionResult RedirectToLocal(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("Index", "Dashboard");
    }
}
