using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Program.Controllers
{
    [Area("Program")]
    public class ProgramController : Controller
    {
        private readonly ProgramService _programService;

        private readonly IMapper _mapper;
        private readonly ILogger<ProgramController> _logger;

        public ProgramController(ProgramService programService, IMapper mapper, ILogger<ProgramController> logger)
        {
            _programService = programService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_programService.GetPrograms());
        }

        public IActionResult View(int id)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            return View(program);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new ProgramInputModel()
            {
                Vision = "Vision",
                Mission = "Mission"
            });
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(ProgramInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var program = _mapper.Map<Models.Program>(input);
            _programService.AddProgram(program);
            _logger.LogInformation("{user} created program {program}", User.Identity.Name, program.Id);

            return RedirectToAction("View", new { id = program.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            ViewBag.Program = program;
            return View(_mapper.Map<ProgramInputModel>(program));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, ProgramInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            _mapper.Map(input, program);
            _programService.SaveChanges();
            _logger.LogInformation("{user} edited program {program}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }
    }
}

namespace Ascent.Models
{
    public class ProgramInputModel
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }

        public string Vision { get; set; }
        public string Mission { get; set; }
    }
}
