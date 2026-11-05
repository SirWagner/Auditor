using System.Text.RegularExpressions;
using Auditor.Models;
using Auditor.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auditor.Controllers
{
    /// <summary>
    /// The lists questions can be answered from: airlines, aircraft types, ground handlers, service providers and any
    /// list users add. Lists are shared by every airport, so users limited to one airport can read them but not change them.
    /// </summary>
    [Authorize(Roles = Roles.AdminOrManager)]
    public class ReferenceListsController : Controller
    {
        private readonly AuditorContext _context;
        private readonly UserScope _scope;

        public ReferenceListsController(AuditorContext context, UserScope scope)
        {
            _context = context;
            _scope = scope;
        }

        private async Task<bool> CanEditAsync() => await _scope.AirportIdAsync() == null;

        public async Task<IActionResult> Index()
        {
            ViewBag.CanEdit = await CanEditAsync();
            var lists = await _context.ReferenceLists
                .Include(l => l.Items)
                .Include(l => l.Questions)
                .OrderBy(l => l.Name)
                .ToListAsync();
            return View(lists);
        }

        public async Task<IActionResult> Details(long id)
        {
            var list = await _context.ReferenceLists
                .Include(l => l.Questions)
                .Include(l => l.Items).ThenInclude(i => i.Airport)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (list == null)
                return NotFound();

            ViewBag.CanEdit = await CanEditAsync();
            ViewBag.Airports = await _context.Airports.OrderBy(a => a.Code).ToListAsync();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? name, string? description)
        {
            if (!await CanEditAsync())
                return Forbid();

            name = Clean(name, 100);
            if (name == null)
            {
                TempData["Error"] = "Enter a name for the list.";
                return RedirectToAction(nameof(Index));
            }
            if (await _context.ReferenceLists.AnyAsync(l => l.Name == name))
            {
                TempData["Error"] = $"A list named \"{name}\" already exists.";
                return RedirectToAction(nameof(Index));
            }

            var list = new ReferenceList { Name = name, Description = Clean(description, 500), CreatedDate = DateTime.UtcNow };
            _context.ReferenceLists.Add(list);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"List \"{name}\" was created. Add its items below.";
            return RedirectToAction(nameof(Details), new { id = list.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            if (!await CanEditAsync())
                return Forbid();

            var list = await _context.ReferenceLists.Include(l => l.Questions).FirstOrDefaultAsync(l => l.Id == id);
            if (list == null)
                return NotFound();

            if (list.Code != null)
            {
                TempData["Error"] = $"\"{list.Name}\" is built in and can't be deleted. You can change its items.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (list.Questions.Count > 0)
            {
                TempData["Error"] = $"\"{list.Name}\" is used by {list.Questions.Count} question(s). Change those questions first.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.ReferenceLists.Remove(list);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"List \"{list.Name}\" was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // One item per line; "Name | CODE" sets the code too.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItems(long id, string? names, string? groupName, long? airportId)
        {
            if (!await CanEditAsync())
                return Forbid();

            var list = await _context.ReferenceLists.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == id);
            if (list == null)
                return NotFound();

            if (airportId != null && !await _context.Airports.AnyAsync(a => a.Id == airportId))
                airportId = null;

            var group = Clean(groupName, 100);
            var order = list.Items.Count == 0 ? 0 : list.Items.Max(i => i.DisplayOrder);
            var added = 0;
            var skipped = 0;

            foreach (var line in (names ?? "").Split('\n'))
            {
                var parts = line.Split('|', 2);
                var name = Clean(parts[0], 200);
                if (name == null)
                    continue;

                if (list.Items.Any(i => i.AirportId == airportId && i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    skipped++;
                    continue;
                }

                list.Items.Add(new ReferenceListItem
                {
                    Name = name,
                    Code = parts.Length > 1 ? Clean(parts[1], 20) : null,
                    GroupName = group,
                    AirportId = airportId,
                    DisplayOrder = ++order,
                    IsActive = true,
                });
                added++;
            }

            await _context.SaveChangesAsync();

            if (added == 0 && skipped == 0)
                TempData["Error"] = "Type at least one name.";
            else
                TempData["Success"] = $"Added {added} item(s)." + (skipped > 0 ? $" {skipped} already on the list." : "");

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(long id, string? name, string? code, string? groupName, long? airportId)
        {
            if (!await CanEditAsync())
                return Forbid();

            var item = await _context.ReferenceListItems.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
                return NotFound();

            name = Clean(name, 200);
            if (name == null)
            {
                TempData["Error"] = "An item needs a name.";
            }
            else if (await _context.ReferenceListItems.AnyAsync(i => i.Id != id && i.ListId == item.ListId && i.AirportId == airportId && i.Name == name))
            {
                TempData["Error"] = $"\"{name}\" is already on this list.";
            }
            else
            {
                item.Name = name;
                item.Code = Clean(code, 20);
                item.GroupName = Clean(groupName, 100);
                item.AirportId = airportId;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"\"{name}\" was saved.";
            }

            return RedirectToAction(nameof(Details), new { id = item.ListId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleItem(long id)
        {
            if (!await CanEditAsync())
                return Forbid();

            var item = await _context.ReferenceListItems.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
                return NotFound();

            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = item.ListId });
        }

        // Answers keep the name that was picked, so removing an item never changes past audits.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(long id)
        {
            if (!await CanEditAsync())
                return Forbid();

            var item = await _context.ReferenceListItems.FirstOrDefaultAsync(i => i.Id == id);
            if (item == null)
                return NotFound();

            _context.ReferenceListItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"\"{item.Name}\" was removed. Past audits keep it.";
            return RedirectToAction(nameof(Details), new { id = item.ListId });
        }

        private static string? Clean(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            value = Regex.Replace(value.Trim(), @"\s+", " ");
            return value.Length > max ? value[..max] : value;
        }
    }
}
