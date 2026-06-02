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
    public class QuestionBanksController : Controller
    {
        private readonly AuditorContext _context;

        public QuestionBanksController(AuditorContext context)
        {
            _context = context;
        }

        // GET: QuestionBanks
        public async Task<IActionResult> Index()
        {
            var auditorContext = _context.QuestionBanks.Include(q => q.Category).Include(q => q.CreatedByNavigation).Include(q => q.QuestionType);
            return View(await auditorContext.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var results = await _context.QuestionBanks
                .Where(q => q.IsActive &&
                       q.QuestionText.Contains(term))
                .Select(q => new
                {
                    id = q.Id,
                    text = q.QuestionText
                })
                .Take(10)
                .ToListAsync();

            return Json(results);
        }

        // GET: QuestionBanks/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBank = await _context.QuestionBanks
                .Include(q => q.Category)
                .Include(q => q.CreatedByNavigation)
                .Include(q => q.QuestionType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionBank == null)
            {
                return NotFound();
            }

            return View(questionBank);
        }

        // GET: QuestionBanks/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.QuestionCategories, "Id", "Name");
            ViewData["CreatedBy"] = new SelectList(_context.AppUsers, "Id", "Id");
            ViewData["QuestionTypeId"] = new SelectList(_context.QuestionTypes, "Id", "Name");
            return View();
        }

        // POST: QuestionBanks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuestionText,CategoryId,QuestionTypeId,ServiceStandardRecommendation,ResponsibleContractor,IsActive,CreatedBy,CreatedDate")] QuestionBank questionBank)
        {
            if (ModelState.IsValid)
            {
                _context.Add(questionBank);
                await _context.SaveChangesAsync();

                // Detect AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Return success message instead of redirect
                    return Json(new
                    {
                        success = true,
                        message = "Audit question has been created successfully!"
                    });
                }
            }

            // Return validation errors for AJAX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return BadRequest(ModelState);
            }

            ViewData["CategoryId"] = new SelectList(_context.QuestionCategories, "Id", "Name", questionBank.CategoryId);
            ViewData["CreatedBy"] = new SelectList(_context.AppUsers, "Id", "Id", questionBank.CreatedBy);
            ViewData["QuestionTypeId"] = new SelectList(_context.QuestionTypes, "Id", "Name", questionBank.QuestionTypeId);

            return View(questionBank);
        }

        // GET: QuestionBanks/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBank = await _context.QuestionBanks.FindAsync(id);
            if (questionBank == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.QuestionCategories, "Id", "Name", questionBank.CategoryId);
            ViewData["CreatedBy"] = new SelectList(_context.AppUsers, "Id", "Id", questionBank.CreatedBy);
            ViewData["QuestionTypeId"] = new SelectList(_context.QuestionTypes, "Id", "Name", questionBank.QuestionTypeId);
            return View(questionBank);
        }

        // POST: QuestionBanks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,QuestionText,CategoryId,QuestionTypeId,ServiceStandardRecommendation,ResponsibleContractor,IsActive,CreatedBy,CreatedDate")] QuestionBank questionBank)
        {
            if (id != questionBank.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionBank);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionBankExists(questionBank.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.QuestionCategories, "Id", "Name", questionBank.CategoryId);
            ViewData["CreatedBy"] = new SelectList(_context.AppUsers, "Id", "Id", questionBank.CreatedBy);
            ViewData["QuestionTypeId"] = new SelectList(_context.QuestionTypes, "Id", "Name", questionBank.QuestionTypeId);
            return View(questionBank);
        }

        // GET: QuestionBanks/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBank = await _context.QuestionBanks
                .Include(q => q.Category)
                .Include(q => q.CreatedByNavigation)
                .Include(q => q.QuestionType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionBank == null)
            {
                return NotFound();
            }

            return View(questionBank);
        }

        // POST: QuestionBanks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var questionBank = await _context.QuestionBanks.FindAsync(id);
            if (questionBank != null)
            {
                _context.QuestionBanks.Remove(questionBank);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionBankExists(long id)
        {
            return _context.QuestionBanks.Any(e => e.Id == id);
        }
    }
}
