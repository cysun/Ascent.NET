using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Rubric.Controllers
{
    [Area("Rubric")]
    public class RubricController : Controller
    {
        private readonly RubricService _rubricService;

        private readonly IMapper _mapper;
        private readonly ILogger<RubricController> _logger;

        public RubricController(RubricService rubricService, IMapper mapper, ILogger<RubricController> logger)
        {
            _rubricService = rubricService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_rubricService.GetRubrics());
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

            var rubric = _mapper.Map<Models.Rubric>(input);
            _rubricService.AddRubric(rubric);
            _logger.LogInformation("{user} created rubric {rubric}", User.Identity.Name, rubric.Id);

            return RedirectToAction("Index");
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
