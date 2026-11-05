using System.Security.Claims;
using System.Text.RegularExpressions;
using Auditor.Models;
using Auditor.Security;
using Auditor.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AppUsersController : Controller
    {
        private readonly AuditorContext _context;
        private readonly PasswordService _passwords;

        public AppUsersController(AuditorContext context, PasswordService passwords)
        {
            _context = context;
            _passwords = passwords;
        }

        // GET: AppUsers
        public async Task<IActionResult> Index(string? q, string? role)
        {
            var users = _context.AppUsers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                users = users.Where(u => u.DisplayName.Contains(q) || u.Email.Contains(q) || u.EmployeeNumber!.Contains(q));
            }

            if (!string.IsNullOrEmpty(role))
                users = users.Where(u => u.Role == role);

            ViewBag.Query = q;
            ViewBag.Role = role;
            await LoadAirportsAsync();
            return View(await users.Include(u => u.Airport).OrderBy(u => u.DisplayName).ToListAsync());
        }

        // GET: AppUsers/Create
        public async Task<IActionResult> Create()
        {
            await LoadAirportsAsync();
            return View(new AppUserFormViewModel { Role = Roles.Auditor });
        }

        // POST: AppUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUserFormViewModel form)
        {
            form.Id = 0;
            await ValidateAsync(form);

            if (!ModelState.IsValid)
                return await FormView(form);

            var user = new AppUser
            {
                AzureAdObjectId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
            };
            Apply(form, user);
            user.PasswordHash = _passwords.Hash(user, form.Password!);

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User \"{user.DisplayName}\" ({user.EmployeeNumber}) was created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AppUsers/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            await LoadAirportsAsync();
            return View(new AppUserFormViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                EmployeeNumber = user.EmployeeNumber,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                AirportId = user.AirportId,
            });
        }

        // POST: AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AppUserFormViewModel form)
        {
            if (id != form.Id)
                return NotFound();

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            await ValidateAsync(form);
            await ValidateNotLockingOutAdminsAsync(user, form);

            if (!ModelState.IsValid)
                return await FormView(form);

            Apply(form, user);
            if (!string.IsNullOrEmpty(form.Password))
                user.PasswordHash = _passwords.Hash(user, form.Password);

            await _context.SaveChangesAsync();

            TempData["Success"] = string.IsNullOrEmpty(form.Password)
                ? $"User \"{user.DisplayName}\" was updated."
                : $"User \"{user.DisplayName}\" was updated and their password was reset.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AppUsers/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(long id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.IsActive)
            {
                if (user.Id == CurrentUserId)
                {
                    TempData["Error"] = "You can't deactivate your own account.";
                    return RedirectToAction(nameof(Index));
                }

                if (user.Role == Roles.Admin && !await OtherActiveAdminExistsAsync(user.Id))
                {
                    TempData["Error"] = "This is the only active administrator. Make someone else an administrator first.";
                    return RedirectToAction(nameof(Index));
                }
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User \"{user.DisplayName}\" is now {(user.IsActive ? "active" : "inactive")}.";
            return RedirectToAction(nameof(Index));
        }

        // Normalizes the input, then checks the rules attributes can't express.
        private async Task ValidateAsync(AppUserFormViewModel form)
        {
            form.DisplayName = Collapse(form.DisplayName);
            form.EmployeeNumber = form.EmployeeNumber?.Trim().ToUpperInvariant();
            form.Email = form.Email?.Trim().ToLowerInvariant();

            ModelState.Clear();
            TryValidateModel(form);

            if (form.AirportId != null && !await _context.Airports.AnyAsync(a => a.Id == form.AirportId))
                ModelState.AddModelError(nameof(form.AirportId), "Select a valid airport, or leave it blank for all airports.");

            if (form.Role != null && !Roles.All.Contains(form.Role))
                ModelState.AddModelError(nameof(form.Role), "Select a valid role.");

            if (form.IsNew || !string.IsNullOrEmpty(form.Password))
            {
                var problem = PasswordService.Validate(form.Password);
                if (problem != null)
                    ModelState.AddModelError(nameof(form.Password), form.IsNew && string.IsNullOrEmpty(form.Password) ? "Password is required." : problem);
                else if (form.Password != form.ConfirmPassword)
                    ModelState.AddModelError(nameof(form.ConfirmPassword), "The passwords don't match.");
            }

            if (!string.IsNullOrEmpty(form.EmployeeNumber)
                && await _context.AppUsers.AnyAsync(u => u.Id != form.Id && u.EmployeeNumber == form.EmployeeNumber))
                ModelState.AddModelError(nameof(form.EmployeeNumber), $"Employee number {form.EmployeeNumber} is already in use.");

            if (!string.IsNullOrEmpty(form.Email)
                && await _context.AppUsers.AnyAsync(u => u.Id != form.Id && u.Email == form.Email))
                ModelState.AddModelError(nameof(form.Email), $"{form.Email} is already in use.");
        }

        private async Task ValidateNotLockingOutAdminsAsync(AppUser user, AppUserFormViewModel form)
        {
            var losesAdmin = user.Role == Roles.Admin && user.IsActive && (form.Role != Roles.Admin || !form.IsActive);
            if (!losesAdmin)
                return;

            if (user.Id == CurrentUserId)
                ModelState.AddModelError(nameof(form.Role), "You can't remove your own administrator access or deactivate yourself.");
            else if (!await OtherActiveAdminExistsAsync(user.Id))
                ModelState.AddModelError(nameof(form.Role), "This is the only active administrator. Make someone else an administrator first.");
        }

        private Task<bool> OtherActiveAdminExistsAsync(long excludingId) =>
            _context.AppUsers.AnyAsync(u => u.Id != excludingId && u.Role == Roles.Admin && u.IsActive);

        private static void Apply(AppUserFormViewModel form, AppUser user)
        {
            user.DisplayName = form.DisplayName!;
            user.EmployeeNumber = form.EmployeeNumber;
            user.Email = form.Email!;
            user.Role = form.Role!;
            user.IsActive = form.IsActive;
            user.AirportId = form.AirportId;
        }

        private async Task LoadAirportsAsync() =>
            ViewBag.Airports = await _context.Airports.OrderBy(a => a.Code).ToListAsync();

        private async Task<IActionResult> FormView(AppUserFormViewModel form)
        {
            await LoadAirportsAsync();
            return View(form);
        }

        private long CurrentUserId =>
            long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        private static string? Collapse(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : Regex.Replace(value.Trim(), @"\s+", " ");
    }
}
