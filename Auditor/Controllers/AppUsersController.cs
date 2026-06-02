using System.Security.Cryptography;
using System.Text;
using Auditor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    public class AppUsersController : Controller
    {
        private readonly AuditorContext _context;

        public AppUsersController(AuditorContext context)
        {
            _context = context;
        }

        // GET: AppUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.AppUsers.ToListAsync());
        }

        // GET: AppUsers/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(new List<string> { "Admin", "Auditor", "Viewer" });
            return View();
        }

        
        // POST: AppUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DisplayName,Email,IsActive")] AppUser user, string password, string role)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email já está em uso.");
                    ViewBag.Roles = new SelectList(new List<string> { "Admin", "Auditor", "Viewer" }, role);
                    return View(user);
                }

                user.PasswordHash = HashPassword(password);
                user.Role = role;
                user.CreatedDate = DateTime.UtcNow;
                user.IsActive = true;
                user.AzureAdObjectId = Guid.NewGuid();

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(new List<string> { "Admin", "Auditor", "Viewer" }, role);
            return View(user);
        }

        // GET: AppUsers/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null) return NotFound();

            var user = await _context.AppUsers.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(new List<string> { "Admin", "Auditor", "Viewer" }, user.Role);
            return View(user);
        }

        // POST: AppUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DisplayName,Email,IsActive")] AppUser user, string role, string? newPassword)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.AppUsers.FindAsync(id);
                if (existing == null) return NotFound();

                existing.DisplayName = user.DisplayName;
                existing.Email = user.Email;
                existing.IsActive = user.IsActive;
                existing.Role = role;

                if (!string.IsNullOrEmpty(newPassword))
                    existing.PasswordHash = HashPassword(newPassword);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(new List<string> { "Admin", "Auditor", "Viewer" }, role);
            return View(user);
        }

        // GET: AppUsers/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null) return NotFound();

            var user = await _context.AppUsers.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: AppUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var user = await _context.AppUsers.FindAsync(id);
            if (user != null)
                _context.AppUsers.Remove(user);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppUserExists(long id)
        {
            return _context.AppUsers.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}