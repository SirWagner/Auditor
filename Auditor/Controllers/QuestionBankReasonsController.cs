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
    public class QuestionBankReasonsController : Controller
    {
        private readonly AuditorContext _context;

        public QuestionBankReasonsController(AuditorContext context)
        {
            _context = context;
        }

        // GET: QuestionBankReasons
        public async Task<IActionResult> Index()
        {
            var auditorContext = _context.QuestionBankReasons.Include(q => q.QuestionBank);
            return View(await auditorContext.ToListAsync());
        }

        // GET: QuestionBankReasons/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBankReason = await _context.QuestionBankReasons
                .Include(q => q.QuestionBank)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionBankReason == null)
            {
                return NotFound();
            }

            return View(questionBankReason);
        }

        // GET: QuestionBankReasons/Create
        public IActionResult Create()
        {
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText");
            return View();
        }

        // POST: QuestionBankReasons/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuestionBankId,ReasonText")] QuestionBankReason questionBankReason)
        {
            if (ModelState.IsValid)
            {
                _context.Add(questionBankReason);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText", questionBankReason.QuestionBankId);
            return View(questionBankReason);
        }

        // GET: QuestionBankReasons/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBankReason = await _context.QuestionBankReasons.FindAsync(id);
            if (questionBankReason == null)
            {
                return NotFound();
            }
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText", questionBankReason.QuestionBankId);
            return View(questionBankReason);
        }

        // POST: QuestionBankReasons/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,QuestionBankId,ReasonText")] QuestionBankReason questionBankReason)
        {
            if (id != questionBankReason.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionBankReason);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionBankReasonExists(questionBankReason.Id))
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
            ViewData["QuestionBankId"] = new SelectList(_context.QuestionBanks, "Id", "QuestionText", questionBankReason.QuestionBankId);
            return View(questionBankReason);
        }

        // GET: QuestionBankReasons/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionBankReason = await _context.QuestionBankReasons
                .Include(q => q.QuestionBank)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionBankReason == null)
            {
                return NotFound();
            }

            return View(questionBankReason);
        }

        // POST: QuestionBankReasons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var questionBankReason = await _context.QuestionBankReasons.FindAsync(id);
            if (questionBankReason != null)
            {
                _context.QuestionBankReasons.Remove(questionBankReason);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionBankReasonExists(long id)
        {
            return _context.QuestionBankReasons.Any(e => e.Id == id);
        }
    }
}
