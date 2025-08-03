using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Program.Controllers
{
    [Area("Program")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class ModuleController : Controller
    {
        private readonly ProgramService _programService;

        private readonly IMapper _mapper;
        private readonly ILogger<ModuleController> _logger;

        public ModuleController(ProgramService programService, IMapper mapper, ILogger<ModuleController> logger)
        {
            _programService = programService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult View(int id)
        {
            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            ViewBag.Program = _programService.GetProgram(module.ProgramId);
            ViewBag.Resources = _programService.GetModuleResources(id);

            return View(module);
        }

        [HttpGet]
        public IActionResult Add(int programId)
        {
            var program = _programService.GetProgram(programId);
            if (program == null) return NotFound();

            ViewBag.Program = program;

            return View(new ProgramModuleInputModel());
        }

        [HttpPost]
        public IActionResult Add(int programId, ProgramModuleInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var module = _mapper.Map<ProgramModule>(input);
            _programService.AddModuleToProgram(programId, module);
            _logger.LogInformation("{user} added module {module} to {program}",
                User.GetName(), module.Id, programId);

            return RedirectToAction("View", "Program", new { id = programId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            ViewBag.Program = _programService.GetProgram(module.ProgramId);
            ViewBag.Module = module;
            ViewBag.Resources = _programService.GetModuleResources(id);

            return View(_mapper.Map<ProgramModuleInputModel>(module));
        }

        [HttpPost]
        public IActionResult Edit(int id, ProgramModuleInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            _mapper.Map(input, module);
            _programService.SaveChanges();
            _logger.LogInformation("{user} edited module {module}", User.GetName(), id);

            return RedirectToAction("View", "Program", new { id = module.ProgramId });
        }

        public IActionResult MoveUp(int id)
        {
            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            _programService.MoveUpModule(module);
            _logger.LogInformation("{user} moved up module {module}", User.GetName(), id);

            return RedirectToAction("Edit", "Program", new { id = module.ProgramId }, $"m-id-{module.Id}");
        }

        public IActionResult MoveDown(int id)
        {
            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            _programService.MoveDownModule(module);
            _logger.LogInformation("{user} moved down module {module}", User.GetName(), id);

            return RedirectToAction("Edit", "Program", new { id = module.ProgramId }, $"m-id-{module.Id}");
        }

        public IActionResult Delete(int id)
        {
            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            _programService.DeleteModule(module);
            _logger.LogInformation("{user} deleted module {module}", User.GetName(), id);

            return RedirectToAction("View", "Program", new { id = module.ProgramId });
        }
    }
}

namespace Ascent.Models
{
    public class ProgramModuleInputModel
    {
        [Required, MaxLength(64)]
        public string Name { get; set; }
    }
}
