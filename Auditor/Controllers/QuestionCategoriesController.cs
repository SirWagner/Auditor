using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;

namespace Auditor.Controllers
{
    public class QuestionCategoriesController : Controller
    {
        private readonly AuditorContext _context;

        public QuestionCategoriesController(AuditorContext context)
        {
            _context = context;
        }

        // GET: QuestionCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.QuestionCategories.ToListAsync());
        }

        // GET: QuestionCategories/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionCategory = await _context.QuestionCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionCategory == null)
            {
                return NotFound();
            }

            return View(questionCategory);
        }

        // GET: QuestionCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: QuestionCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] QuestionCategory questionCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(questionCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(questionCategory);
        }

        // GET: QuestionCategories/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionCategory = await _context.QuestionCategories.FindAsync(id);
            if (questionCategory == null)
            {
                return NotFound();
            }
            return View(questionCategory);
        }

        // POST: QuestionCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Name,Description")] QuestionCategory questionCategory)
        {
            if (id != questionCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionCategoryExists(questionCategory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(questionCategory);
        }

        // GET: QuestionCategories/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionCategory = await _context.QuestionCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionCategory == null)
            {
                return NotFound();
            }

            return View(questionCategory);
        }

        // POST: QuestionCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var questionCategory = await _context.QuestionCategories.FindAsync(id);
            if (questionCategory != null)
            {
                _context.QuestionCategories.Remove(questionCategory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionCategoryExists(long id)
        {
            return _context.QuestionCategories.Any(e => e.Id == id);
        }
    }
}
