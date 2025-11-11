using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Program.Controllers
{
    [Area("Program")]
    public class ProgramController : Controller
    {
        private readonly ProgramService _programService;
        private readonly PageService _pageService;

        private readonly AppMapper _mapper;
        private readonly ILogger<ProgramController> _logger;

        public ProgramController(ProgramService programService, PageService pageService,
            AppMapper mapper, ILogger<ProgramController> logger)
        {
            _programService = programService;
            _pageService = pageService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_programService.GetPrograms());
        }

        [AllowAnonymous]
        public IActionResult View(int id)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            ViewBag.Modules = _programService.GetModules(id);
            ViewBag.Resources = _programService.GetProgramResources(id);

            return View(program);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new ProgramInputModel()
            {
                HasObjectives = true,
                Objectives = new List<string>() { "", "", "", "" },
                Outcomes = new List<string>() { "", "", "", "" }
            });
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(ProgramInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var program = _mapper.Map(input);
            if (input.HasObjectives)
                program.ObjectivesDescription = new Page { Subject = $"{input.Name} Objectives Description" };
            
            _programService.AddProgram(program);
            _logger.LogInformation("{user} created program {program}", User.GetName(), program.Id);

            return RedirectToAction("View", new { id = program.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            ViewBag.Program = program;
            ViewBag.Modules = _programService.GetModules(id);

            return View(_mapper.Map(program));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, ProgramInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            _mapper.Map(input, program);
            if (input.HasObjectives && program.ObjectivesDescriptionId == null)
                program.ObjectivesDescription = new Page { Subject = $"{input.Name} Objectives Description" };
            
            _programService.SaveChanges();
            _logger.LogInformation("{user} edited program {program}", User.GetName(), id);

            return RedirectToAction("View", new { id });
        }

        [AllowAnonymous]
        public IActionResult Objectives(int id)
        {
            var program = _programService.GetProgram(id);
            if (program == null || !program.HasObjectives) return NotFound();

            ViewBag.Page = _pageService.GetPage((int)program.ObjectivesDescriptionId);

            return View(program);
        }

        [AllowAnonymous]
        public IActionResult Outcome(int id, int index)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            ViewBag.Page = _pageService.GetPage(program.Outcomes[index].DescriptionId);

            return View(program);
        }
    }
}

namespace Ascent.Models
{
    public class ProgramInputModel
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }

        public bool HasObjectives { get; set; }
        public List<string> Objectives { get; set; } = new List<string>();

        public List<string> Outcomes { get; set; }
    }
}
