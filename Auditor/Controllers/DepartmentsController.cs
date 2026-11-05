using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;

namespace Auditor.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Auditor.Security.Roles.AdminOrManager)]
    public class DepartmentsController : Controller
    {
        private readonly AuditorContext _context;

        public DepartmentsController(AuditorContext context)
        {
            _context = context;
        }

        // GET: Departments
        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments
                .Include(d => d.AuditSites)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(departments);
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            var department = await _context.Departments
                .Include(d => d.AuditSites)
                    .ThenInclude(s => s.Airport)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            return View(new Department());
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Department department)
        {
            await ValidateAsync(department);

            if (!ModelState.IsValid)
                return View(department);

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Department \"{department.Name}\" was created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Description")] Department form)
        {
            if (id != form.Id)
                return NotFound();

            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound();

            await ValidateAsync(form);

            if (!ModelState.IsValid)
                return View(form);

            department.Name = form.Name;
            department.Description = form.Description;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Department \"{department.Name}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Departments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            var department = await _context.Departments
                .Include(d => d.AuditSites)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return RedirectToAction(nameof(Index));

            if (department.AuditSites.Count > 0)
            {
                TempData["Error"] = $"\"{department.Name}\" is used by {department.AuditSites.Count} audit site(s) and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Department \"{department.Name}\" was deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateAsync(Department department)
        {
            department.Name = string.IsNullOrWhiteSpace(department.Name) ? null! : Regex.Replace(department.Name.Trim(), @"\s+", " ");
            department.Description = string.IsNullOrWhiteSpace(department.Description) ? null! : department.Description.Trim();

            // The entity's non-nullable strings make MVC treat Description as implicitly required; validate by hand instead.
            ModelState.Clear();

            if (string.IsNullOrEmpty(department.Name))
                ModelState.AddModelError(nameof(department.Name), "Department Name is required.");
            else if (department.Name.Length > 255)
                ModelState.AddModelError(nameof(department.Name), "Department Name cannot exceed 255 characters.");
            else if (await _context.Departments.AnyAsync(d => d.Id != department.Id && d.Name == department.Name))
                ModelState.AddModelError(nameof(department.Name), $"A department named \"{department.Name}\" already exists.");

            if (department.Description?.Length > 500)
                ModelState.AddModelError(nameof(department.Description), "Description cannot exceed 500 characters.");
        }
    }
}
