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
        private readonly IAuditTemplateService _service;
        private readonly IAppUserService _appUserService;
        private readonly IQuestionBankService _questionBankService;

        public AuditTemplatesController(IAuditTemplateService service,
                                        IAppUserService AppUserService,
                                        IQuestionBankService QuestionBankService)
        {
            _service = service;
            _appUserService = AppUserService;
            _questionBankService = QuestionBankService;
        }
        public async Task<IActionResult> Index()
        {
            

            var templates = await _service.GetAllAsync();
            return View(templates);
        }

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
            var template = await _service.GetById(id);
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditTemplateCreateDTO AuditTemplateDTO)
        {
            try
            {
                await _service.CreateAsync(AuditTemplateDTO);

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
            var vm = await _service.GetEditViewModelAsync(id);

            if (vm == null)
                return NotFound();

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AuditTemplateEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(await _service.GetEditViewModelAsync(model.Id));

            await _service.UpdateAsync(model);

            return RedirectToAction(nameof(Index));
        }



    }
}

