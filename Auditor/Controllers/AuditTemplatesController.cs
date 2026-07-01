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
            try
            {
                //TODO => Grab the current user to know who created tje AuditTemplate
                var randomUser = await _appUserService.GetAll();
                var Questions = AuditTemplate.Items
                                .Select(item=>new AuditTemplateItemsDTO(item.QuestionBankId,item.Mandatory, item.Sequence)).ToList();
                var AuditTemplateDTO = new AuditTemplateCreateDTO(AuditTemplate.Name,
                                                           AuditTemplate.Description, AuditTemplate.Version, 
                    AuditTemplate.IsActive, randomUser.First().Id, Questions);

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
                Name = template.Name,
                Description = template.Description,
                Version = template.Version,
                IsActive = template.IsActive,

                Users = Users
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.DisplayName
                    })
                    .ToList(),
                QuestionBank = QuestionBanks
                   .Select(q => new AuditTemplateEditQuestionBankViewModel
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
        public async Task<IActionResult> Edit(AuditTemplateEditViewModel model)
        {
            //if (!ModelState.IsValid)
            //    return View(await _auditTemplateService.GetEditViewModelAsync(model.Id));

            //await _auditTemplateService.UpdateAsync(model);

            return RedirectToAction(nameof(Index));
        }



    }
}

