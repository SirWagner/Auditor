using Auditor.DTO.AppUsers;
using Auditor.DTO.AuditTemplate;
using Auditor.Models;
using Auditor.Services.Interfaces;
using Auditor.ViewModels.AuditTemplate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auditor.Controllers
{
    public class AuditTemplatesController : Controller
    {
        private readonly IAuditTemplateService _auditTemplateService;
        private readonly IAppUserService _appUserService;
        private readonly IQuestionBankService _questionBankService;

        public AuditTemplatesController(IAuditTemplateService service,
                                        IAppUserService AppUserService,
                                        IQuestionBankService QuestionBankService)
        {
            _auditTemplateService = service;
            _appUserService = AppUserService;
            _questionBankService = QuestionBankService;
        }
        public async Task<IActionResult> Index()
        {
            

            var templates = await _auditTemplateService.GetAllAsync();
            return View(templates);
        }

        //This is for displaying the AuditTemplate page
        public async Task<IActionResult> Create()
        {


            var Users = await _appUserService.GetAll();
            var QuestionBanks = await _questionBankService.GetAll();

            var vm = new AuditTemplateCreateViewModel
            {
                Users = Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.DisplayName
                    })
                    .ToList(),

                //QuestionBankTypes = await _context.QuestionTypes
                //    .Select(q => new SelectListItem
                //    {
                //        Value = q.Id.ToString(),
                //        Text = q.Name
                //    })
                //    .ToListAsync(),
                QuestionBank = QuestionBanks
                   .Select(q => new QuestionBankListViewModel
                   {
                       Id = q.Id,
                       Text = q.QuestionText,
                       QuestionType = q.QuestionType,  // or however you reference the type
                       Description = q.Category
                   })
                   .ToList()
            };

            return View(vm);
        }
        public async Task<IActionResult> Details(long id)
        {

            var template = await _auditTemplateService.GetById(id);

            var vm = new AuditTemplateDetailsViewModel
            {
                Name = template.Name,
                Description = template.Description,
                Version = template.Version,
                IsActive = template.IsActive,
                CreatedBy = template.UserDisplayName,

                Items = template.ItemsDetails
                    .OrderBy(i => i.Sequence)
                    .Select(i => new AuditTemplateDetailsQuestionsViewModel
                    {
                        Mandatory = i.Mandatory,
                        Sequence = i.Sequence,
                        QuestionText = i.QuestionText,
                        Category = i.Category,
                        Type = i.Type,
                    }).ToList(),
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuditTemplateRequest AuditTemplate)
        {
            //if (!TryGetCurrentUserId(out var userId))
            //{
            //    ModelState.AddModelError("", "User information could not be retrieved.");
            //    return await Create();
            //}
            var userId = 1;//TODO change this to grab current user
            try
            {
                var Questions = AuditTemplate.Items
                                .Select(item=>new AuditTemplateItemsDTO(item.QuestionBankId,item.Mandatory, item.Sequence)).ToList();
                var AuditTemplateDTO = new AuditTemplateCreateDTO(AuditTemplate.Name,
                                                           AuditTemplate.Description, AuditTemplate.Version,
                    AuditTemplate.IsActive, userId, Questions);

                await _auditTemplateService.CreateAsync(AuditTemplateDTO);

            }
            catch (DbUpdateException ex)
            {
                return Problem(
                     detail: ex.Message,
                    title: "Internal Server Error",
                    statusCode: 500
                    );
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long id)
        {
            var template = await _auditTemplateService.GetById(id);

            var Users = await _appUserService.GetAll();
            var QuestionBanks = await _questionBankService.GetAll();

            var vm = new AuditTemplateEditViewModel
            {
                Id=id,
                AuditTemplateInfoViewModel = new AuditTemplateInfoViewModel()
                {
                    Name = template.Name,
                    Description = template.Description,
                    Version = template.Version,
                    IsActive = template.IsActive,
                },
                AuditTemplateItems= template.ItemsDetails.Select(
                    selectedQuestion =>new AuditTemplateItemViewModel()
                    {
                        Mandatory= selectedQuestion.Mandatory,
                        QuestionBankId = selectedQuestion.Id, 
                        QuestionText = selectedQuestion.QuestionText,
                        Sequence = selectedQuestion.Sequence
                    }
                    ).ToList(),
                Users = Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.DisplayName
                    })
                    .ToList(),
                QuestionBank = QuestionBanks
                   .Select(q => new QuestionBankListViewModel
                   {
                       Id = q.Id,
                       Text = q.QuestionText,
                       QuestionType = q.QuestionType,  // or however you reference the type
                       Description = q.Category
                   })
                   .ToList()
            };

            if (vm == null)
                return NotFound();

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAuditTemplateRequest AuditTemplate)
        {
            //if (!ModelState.IsValid)
            //    return View(await _auditTemplateService.GetEditViewModelAsync(model.Id));

            //if (!TryGetCurrentUserId(out var userId))
            //{
            //    ModelState.AddModelError("", "User information could not be retrieved.");
            //    return await Edit(AuditTemplate.Id);
            //}
            var userId = 1;//TODO change this to grab current user

            try
            {
                var Questions = AuditTemplate.Items
                                .Select(item => new AuditTemplateEditItemsDTO(item.QuestionBankId, item.Mandatory, item.Sequence)).ToList();
                var AuditTemplateDTO = new AuditTemplateEditDTO(AuditTemplate.Id, AuditTemplate.Name,
                                                           AuditTemplate.Description, AuditTemplate.Version,
                    AuditTemplate.IsActive, userId, Questions);

                await _auditTemplateService.UpdateAsync(AuditTemplateDTO);

            }
            catch (DbUpdateException ex)
            {
                return Problem(
                     detail: ex.Message,
                    title: "Internal Server Error",
                    statusCode: 500
                    );
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: AuditTemplates/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var template = await _auditTemplateService.GetById(id);

            if (template == null)
                return NotFound();

            var vm = new AuditTemplateDeleteViewModel
            {
                Id = id,
                Name = template.Name,
                Description = template.Description,
                Version = template.Version,
                IsActive = template.IsActive,
                CreatedByName = template.UserDisplayName,
                QuestionCount = template.ItemsDetails.Count
            };

            return View(vm);
        }

        // POST: AuditTemplates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var deleted = await _auditTemplateService.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "This template cannot be deleted because it is referenced by one or more audit schedules.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(long id)
        {
            if (!TryGetCurrentUserId(out var userId))
                return RedirectToAction(nameof(Index));

            await _auditTemplateService.ToggleActiveAsync(id, userId);

            return RedirectToAction(nameof(Index));
        }

        private bool TryGetCurrentUserId(out long userId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(claim, out userId);
        }
    }
}

