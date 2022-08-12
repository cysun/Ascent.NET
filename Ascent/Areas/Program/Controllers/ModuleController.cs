using System.ComponentModel.DataAnnotations;
using Ascent.Models;
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

        public IActionResult Index(int programId)
        {
            var program = _programService.GetProgram(programId);
            if (program == null) return NotFound();

            ViewBag.Program = program;

            return View(_programService.GetModules(programId));
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
                User.Identity.Name, module.Id, programId);

            return RedirectToAction("Index", "Module", new { programId }, $"m-id-{module.Id}");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            ViewBag.Program = _programService.GetProgram(module.ProgramId);
            ViewBag.Items = _programService.GetItems(id);

            return View(module);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProgramModuleInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var module = _programService.GetModule(id);
            if (module == null) return NotFound();

            _mapper.Map(input, module);
            _programService.SaveChanges();
            _logger.LogInformation("{user} edited module {module}", User.Identity.Name, id);

            return RedirectToAction("Index", "Module", new { programId = module.ProgramId }, $"m-id-{module.Id}");
        }

        public IActionResult AddItem(int id, int contentId, string type)
        {
            var item = new ProgramItem();
            switch (type?.ToLower())
            {
                case "file":
                    item.Type = ItemType.File;
                    item.FileId = contentId;
                    break;
                case "page":
                    item.Type = ItemType.Page;
                    item.PageId = contentId;
                    break;
                default:
                    _logger.LogInformation("Unrecognized item type: {type}", type);
                    return BadRequest();
            }

            _programService.AddItemToModule(id, item);
            _logger.LogInformation("{user} added item {item} to module {module}", User.Identity.Name, item.Id, id);
            return RedirectToAction("Edit", new { id });
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
