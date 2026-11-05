using Auditor.DTO.Questionbanks;
using Auditor.Models;
using Auditor.Services;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.QuestionBank;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Auditor.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = Auditor.Security.Roles.AdminOrManager)]
    public class QuestionBanksController : Controller
    {
        private readonly AuditorContext _context;
        private readonly IQuestionBankService _questionBankService;

        public QuestionBanksController(AuditorContext context, IQuestionBankService questionBankService)
        {
            _context = context;
            this._questionBankService = questionBankService;
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
            FillCreateOptions();
            return View();
        }

        private void FillCreateOptions()
        {
            ViewData["CategoryId"] = new SelectList(_context.QuestionCategories, "Id", "Name");
            ViewData["CreatedBy"] = new SelectList(_context.AppUsers, "Id", "Id");
            ViewData["QuestionTypeId"] = new SelectList(_context.QuestionTypes, "Id", "Name");
            ViewBag.ReferenceLists = _context.ReferenceLists.OrderBy(l => l.Name).ToList();
        }

        // A LIST question needs a list to answer from.
        private async Task ValidateListAsync(long questionTypeId, long? referenceListId)
        {
            var type = await _context.QuestionTypes.Where(t => t.Id == questionTypeId).Select(t => t.Name).FirstOrDefaultAsync();
            if (type == "LIST" && (referenceListId == null || !await _context.ReferenceLists.AnyAsync(l => l.Id == referenceListId)))
                ModelState.AddModelError("ReferenceListId", "Select the list this question is answered from.");
        }

        // POST: QuestionBanks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionBankCreateViewModel model)
        {


            await ValidateListAsync(model.QuestionTypeId, model.ReferenceListId);
            if (!ModelState.IsValid)
            {
                FillCreateOptions();
                return View(model);
            }


            var dto = new CreateQuestionBankDTO
            {
                QuestionText = model.QuestionText,

                CategoryId = model.CategoryId,

                QuestionTypeId = model.QuestionTypeId,

                ServiceStandardRecommendation =
                    model.ServiceStandardRecommendation,

                ResponsibleContractor =
                    model.ResponsibleContractor,

                ReferenceListId = model.ReferenceListId,

                IsActive = model.IsActive,

                CreatedBy = model.CreatedBy,


                Options = model.Options
                .Select(item => new CreateQuestionOptionDTO
                {
                    Text = item.Text,

                    Sequence = item.Sequence

                })
                .ToList()
            };


            await _questionBankService.Create(dto);


            return RedirectToAction(nameof(Index));
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
            ViewBag.ReferenceLists = await _context.ReferenceLists.OrderBy(l => l.Name).ToListAsync();
            return View(questionBank);
        }

        // POST: QuestionBanks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,QuestionText,CategoryId,QuestionTypeId,ServiceStandardRecommendation,ResponsibleContractor,ReferenceListId,IsActive,CreatedBy,CreatedDate")] QuestionBank questionBank)
        {
            if (id != questionBank.Id)
            {
                return NotFound();
            }

            await ValidateListAsync(questionBank.QuestionTypeId, questionBank.ReferenceListId);
            if (await _context.QuestionTypes.Where(t => t.Id == questionBank.QuestionTypeId).Select(t => t.Name).FirstOrDefaultAsync() != "LIST")
                questionBank.ReferenceListId = null;

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
            ViewBag.ReferenceLists = await _context.ReferenceLists.OrderBy(l => l.Name).ToListAsync();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(long id)
        {
            //if (!TryGetCurrentUserId(out var userId))
            //    return RedirectToAction(nameof(Index));
            var userId = 1;
            // TODO - Grab the current user

            await _questionBankService.ToggleActiveAsync(id, userId);

            return RedirectToAction(nameof(Index));
        }
    }
}
