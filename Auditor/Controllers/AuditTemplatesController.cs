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

        public AuditTemplatesController(IAuditTemplateService service)
        {
            _service = service;
        }
        public async Task<IActionResult> Index()
        {
            var templates = await _service.GetAllAsync();
            return View(templates);
        }

        public async Task<IActionResult> Create()
        {
            var vm = await _service.GetCreateViewModelAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditTemplateCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await _service.GetCreateViewModelAsync());
            }

            await _service.CreateAsync(model);
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
