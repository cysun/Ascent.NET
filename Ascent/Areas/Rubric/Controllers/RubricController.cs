using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Rubric.Controllers
{
    [Area("Rubric")]
    public class RubricController : Controller
    {
        private readonly RubricService _rubricService;

        private readonly AppMapper _mapper;
        private readonly ILogger<RubricController> _logger;

        public RubricController(RubricService rubricService, AppMapper mapper, ILogger<RubricController> logger)
        {
            _rubricService = rubricService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View(_rubricService.GetRubrics());
        }

        [AllowAnonymous]
        public IActionResult View(int id)
        {
            var rubric = _rubricService.GetRubric(id);
            if (rubric == null) return NotFound();

            ViewBag.Criteria = _rubricService.GetCriteria(id);

            return View(rubric);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new RubricInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(RubricInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var rubric = _mapper.Map(input);
            _rubricService.AddRubric(rubric);
            _logger.LogInformation("{user} created rubric {rubric}", User.GetName(), rubric.Id);

            return RedirectToAction("Index", "Criterion", new { rubricId = rubric.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var rubric = _rubricService.GetRubric(id);
            if (rubric == null) return NotFound();

            ViewBag.Rubric = rubric;
            return View(_mapper.Map(rubric));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, RubricInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var rubric = _rubricService.GetRubric(id);
            if (rubric == null) return NotFound();

            _mapper.Map(input, rubric);
            _rubricService.SaveChanges();
            _logger.LogInformation("{user} edited rubric {rubric}", User.GetName(), id);

            return RedirectToAction("View", new { id });
        }
    }
}

namespace Ascent.Models
{
    public class RubricInputModel
    {
        [Required, MaxLength(80)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Publish Time")]
        public DateTime? TimePublished { get; set; }

        public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;
    }
}
