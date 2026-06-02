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
    public class QuestionBankOptionsController : Controller
    {
        private readonly AuditorContext _context;

        public QuestionBankOptionsController(AuditorContext context)
        {
            _context = context;
        }

        // GET: QuestionBankOptions
        public async Task<IActionResult> Index()
        {
            var auditorContext = _context.QuestionBankOptions.Include(q => q.QuestionBank);
            return View(await auditorContext.ToListAsync());
        }

        // GET: QuestionBankOptions/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBankOption = await _context.QuestionBankOptions
                .Include(q => q.QuestionBank)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionBankOption == null)
            {
                return NotFound();
            }

            return View(questionBankOption);
        }

        // GET: QuestionBankOptions/Create
        public IActionResult Create()
        {
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText");
            return View();
        }

        // POST: QuestionBankOptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuestionBankId,OptionText,OptionValue,DisplayOrder")] QuestionBankOption questionBankOption)
        {
            if (ModelState.IsValid)
            {
                _context.Add(questionBankOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText", questionBankOption.QuestionBankId);
            return View(questionBankOption);
        }

        // GET: QuestionBankOptions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBankOption = await _context.QuestionBankOptions.FindAsync(id);
            if (questionBankOption == null)
            {
                return NotFound();
            }
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText", questionBankOption.QuestionBankId);
            return View(questionBankOption);
        }

        // POST: QuestionBankOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,QuestionBankId,OptionText,OptionValue,DisplayOrder")] QuestionBankOption questionBankOption)
        {
            if (id != questionBankOption.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionBankOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionBankOptionExists(questionBankOption.Id))
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
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText", questionBankOption.QuestionBankId);
            return View(questionBankOption);
        }

        // GET: QuestionBankOptions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBankOption = await _context.QuestionBankOptions
                .Include(q => q.QuestionBank)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionBankOption == null)
            {
                return NotFound();
            }

            return View(questionBankOption);
        }

        // POST: QuestionBankOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var questionBankOption = await _context.QuestionBankOptions.FindAsync(id);
            if (questionBankOption != null)
            {
                _context.QuestionBankOptions.Remove(questionBankOption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionBankOptionExists(long id)
        {
            return _context.QuestionBankOptions.Any(e => e.Id == id);
        }
    }
}
