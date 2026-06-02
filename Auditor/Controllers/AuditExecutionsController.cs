using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auditor.Models;
using Auditor.ViewModels.AuditExecution;

namespace Auditor.Controllers
{
    public class AuditExecutionsController : Controller
    {
        private readonly AuditorContext _context;

        public AuditExecutionsController(AuditorContext context)
        {
            _context = context;
        }

        /*
                ========================================
                START AUDIT
                Creates execution record
                ========================================
                */

        public async Task<IActionResult> Start(long scheduleId)
        {
            var schedule = await _context.AuditSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                return NotFound();

            var execution = new AuditExecution
            {
                ScheduleId = scheduleId,
                AuditorId = 1, // TODO replace later with logged user 
                Status = "IN_PROGRESS",
                OriginalAuditDate = schedule.ScheduledDate
            };

            _context.AuditExecutions.Add(execution);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Execute",
                new { executionId = execution.Id }
            );
        }

        /*
        ========================================
        EXECUTE AUDIT
        ========================================
        */

        public async Task<IActionResult> Execute(long executionId)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Site)
                .Include(a=>a.Auditor)
                .FirstOrDefaultAsync(e => e.Id == executionId);

            if (execution == null)
                return NotFound();

            var templateItems = await _context.AuditTemplateItems
                .Include(t => t.QuestionBank)
                    .ThenInclude(q => q.QuestionType)
                .Include(t => t.QuestionBank)
                    .ThenInclude(q => q.QuestionBankOptions)
                .Include(t => t.QuestionBank)
                    .ThenInclude(q => q.QuestionBankReasons)
                .Where(t => t.TemplateId == execution.Schedule.TemplateId)
                .OrderBy(t => t.Sequence)
                .ToListAsync();

            var vm = new AuditExecutionRenderViewModel
            {
                ExecutionId = execution.Id,
                AuditName = execution.Schedule.Template.Name,
                SiteName = execution.Schedule.Site.Name,
                
                Questions = templateItems.Select(t => new QuestionViewModel
                {
                    TemplateItemId = t.Id,
                    QuestionText = t.QuestionBank.QuestionText,
                    QuestionTypeId = t.QuestionBank.QuestionTypeId,
                    QuestionTypeName = t.QuestionBank.QuestionType.Name, 
                    IsMandatory = t.Mandatory,
                    RequiresReason = t.QuestionBank.QuestionBankOptions
                                        .Any(o => o.RequiresReason),
                    Options = t.QuestionBank.QuestionBankOptions.ToList(),
                    ReasonOptions = t.QuestionBank.QuestionBankReasons.ToList()
                }).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> Index()
        {
            var audits = await _context.AuditExecutions
                .Include(x => x.Schedule)
                    .ThenInclude(s => s.Template)
                .Include(x => x.Schedule)
                    .ThenInclude(s => s.Site)
                .Include(x => x.Auditor)
                .ToListAsync();

            return View(audits);
        }


        [HttpPost]
        public async Task<IActionResult> Finalize(long id)
        {
            var execution = await _context.AuditExecutions
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null)
                return Json(new { success = false, message = "Execution not found." });

            if (execution.Status != "COMPLETED")
                return Json(new { success = false, message = "Only completed audits can be finalized." });

            execution.Status = "CLOSED";
            execution.SubmissionDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        /*
        -------------------------------------------------
        PAGE 2
        Audit Details
        -------------------------------------------------
        */

        // GET: AuditExecution/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var execution = await _context.AuditExecutions
                .Include(e => e.Auditor)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                        .ThenInclude(t => t.AuditTemplateItems)
                            .ThenInclude(i => i.QuestionBank)
                                .ThenInclude(q => q.QuestionType)
                .Include(e => e.Schedule)
                    .ThenInclude(s => s.Template)
                        .ThenInclude(t => t.AuditTemplateItems)
                            .ThenInclude(i => i.QuestionBank)
                                .ThenInclude(q => q.QuestionBankOptions)
                .Include(e => e.AuditResponses)
                    .ThenInclude(r => r.SelectedOption)
                .Include(e => e.AuditResponses)
                    .ThenInclude(r => r.Reasons)
                .Include(e => e.AuditAttachments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (execution == null)
                return NotFound();

            return View(execution);
        }
        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] AuditSubmissionModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine("===== AUDIT SUBMISSION START =====");

                var execution = await _context.AuditExecutions
                    .FirstOrDefaultAsync(x => x.Id == model.ExecutionId);

                if (execution == null)
                {
                    return Json(new { success = false, message = "Execution not found." });
                }

                if (execution.Status == "COMPLETED")
                {
                    return Json(new { success = false, message = "Audit already completed." });
                }

                // Preload template items to get QuestionId
                var templateItemIds = model.Responses.Select(r => r.TemplateItemId).ToList();

                var templateItems = await _context.AuditTemplateItems
                    .Where(x => templateItemIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

                foreach (var response in model.Responses)
                {
                    if (!templateItems.ContainsKey(response.TemplateItemId))
                        continue;

                    var templateItem = templateItems[response.TemplateItemId];

                    var entity = new AuditResponse
                    {
                        ExecutionId = execution.Id,
                        TemplateItemId = response.TemplateItemId,
                        SelectedOptionId = response.SelectedOptionId,
                        Comment = response.Comment,
                        Compliant = response.Compliant
                    };

                    _context.AuditResponses.Add(entity);

                    // Save first to get Response ID
                    await _context.SaveChangesAsync();

                    var reasonsToAttach = new List<QuestionBankReason>();

                    // Attach existing reasons
                    if (response.SelectedReasonIds != null && response.SelectedReasonIds.Any())
                    {
                        var existingReasons = await _context.QuestionBankReasons
                            .Where(r => response.SelectedReasonIds.Contains(r.Id))
                            .ToListAsync();

                        reasonsToAttach.AddRange(existingReasons);
                    }

                    // Create custom reason
                    if (!string.IsNullOrWhiteSpace(response.CustomReason))
                    {
                        Console.WriteLine("Creating custom reason...");

                        var newReason = new QuestionBankReason
                        {
                            QuestionBankId = templateItem.QuestionBankId,
                            ReasonText = response.CustomReason
                        };

                        _context.QuestionBankReasons.Add(newReason);

                        await _context.SaveChangesAsync();

                        reasonsToAttach.Add(newReason);

                        Console.WriteLine($"Custom reason created ID {newReason.Id}");
                    }

                    // Attach reasons to response
                    foreach (var reason in reasonsToAttach)
                    {
                        entity.Reasons.Add(reason);
                    }

                    await _context.SaveChangesAsync();
                }

                execution.Status = "COMPLETED";

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                Console.WriteLine("===== AUDIT SAVED SUCCESSFULLY =====");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine("ERROR: " + ex.Message);

                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // GET: AuditExecutions/Create
        public IActionResult Create()
        {
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "Id");
            ViewData["ScheduleId"] = new SelectList(_context.AuditSchedules, "Id", "Status");
            return View();
        }

        // POST: AuditExecutions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,ScheduleId,AuditorId,Status,AcceptanceDate,RejectionReason,SubmissionDate,OriginalAuditDate")] AuditExecution auditExecution)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(auditExecution);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "Id", auditExecution.AuditorId);
        //    ViewData["ScheduleId"] = new SelectList(_context.AuditSchedules, "Id", "Status", auditExecution.ScheduleId);
        //    return View(auditExecution);
        //}

        // GET: AuditExecutions/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditExecution = await _context.AuditExecutions.FindAsync(id);
            if (auditExecution == null)
            {
                return NotFound();
            }
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "Id", auditExecution.AuditorId);
            ViewData["ScheduleId"] = new SelectList(_context.AuditSchedules, "Id", "Status", auditExecution.ScheduleId);
            return View(auditExecution);
        }

        // POST: AuditExecutions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,ScheduleId,AuditorId,Status,AcceptanceDate,RejectionReason,SubmissionDate,OriginalAuditDate")] AuditExecution auditExecution)
        {
            if (id != auditExecution.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditExecution);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditExecutionExists(auditExecution.Id))
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
            ViewData["AuditorId"] = new SelectList(_context.AppUsers, "Id", "Id", auditExecution.AuditorId);
            ViewData["ScheduleId"] = new SelectList(_context.AuditSchedules, "Id", "Status", auditExecution.ScheduleId);
            return View(auditExecution);
        }

        // GET: AuditExecutions/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditExecution = await _context.AuditExecutions
                .Include(a => a.Auditor)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditExecution == null)
            {
                return NotFound();
            }

            return View(auditExecution);
        }

        // POST: AuditExecutions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var auditExecution = await _context.AuditExecutions.FindAsync(id);
            if (auditExecution != null)
            {
                _context.AuditExecutions.Remove(auditExecution);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditExecutionExists(long id)
        {
            return _context.AuditExecutions.Any(e => e.Id == id);
        }
    }
}
