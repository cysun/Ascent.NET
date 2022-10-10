using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Rubric.Controllers
{
    [Area("Rubric")]
    public class CriterionController : Controller
    {
        private readonly RubricService _rubricService;

        private readonly IMapper _mapper;
        private readonly ILogger<CriterionController> _logger;

        public CriterionController(RubricService rubricService, IMapper mapper, ILogger<CriterionController> logger)
        {
            _rubricService = rubricService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(int rubricId)
        {
            var rubric = _rubricService.GetRubric(rubricId);
            if (rubric == null) return NotFound();

            ViewBag.Rubric = rubric;

            return View(_rubricService.GetCriteria(rubricId));
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(int rubricId)
        {
            var rubric = _rubricService.GetRubric(rubricId);
            if (rubric == null) return NotFound();

            ViewBag.Rubric = rubric;

            return View(new RubricCriterionInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(int rubricId, RubricCriterionInputModel input)
        {
            // For more commonly used scales, Google "sample rubric scales"
            var scales = new Dictionary<int, string[]>()
            {
                {3, new string[]{"Developing", "Competent", "Exemplary"} },
                {4, new string[]{"Unsatisfactory", "Needs Improvement", "Meets Expectations", "Exceeds Expectations"} },
                {5, new string[]{"Poor", "Minimal", "Sufficient", "Above Average", "Excellent"} }
            };
            var criterion = new RubricCriterion
            {
                Name = input.Name,
                Ratings = new List<RubricRating>()
            };
            for (var i = 0; i < input.Scale; ++i)
                criterion.Ratings.Add(new RubricRating
                {
                    Index = i,
                    Value = i + 1,
                    Name = scales.ContainsKey(input.Scale) ? scales[input.Scale][i] : ""
                });

            _rubricService.AddCriterionToRubric(rubricId, criterion);
            _logger.LogInformation("{user} added criterion {criterion} to rubric {rubric}",
                User.Identity.Name, criterion.Id, rubricId);

            return RedirectToAction("Edit", new { id = criterion.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var criterion = _rubricService.GetCriterion(id);
            if (criterion == null) return NotFound();

            ViewBag.Criterion = criterion;

            return View(_mapper.Map<RubricCriterionInputModel>(criterion));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, RubricCriterionInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var criterion = _rubricService.GetCriterion(id);
            if (criterion == null) return NotFound();

            _mapper.Map(input, criterion);
            _rubricService.SaveChanges();
            _logger.LogInformation("{user} edited criterion {criterion}", User.Identity.Name, id);

            return RedirectToAction("Index", "Criterion", new { rubricId = criterion.RubricId });
        }
    }
}

namespace Ascent.Models
{
    public class RubricCriterionInputModel
    {
        [Required, MaxLength(80)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int RubricId { get; set; }

        public int Index { get; set; }

        public int Scale { get; set; } = 4;

        public List<RubricRatingInputModel> Ratings { get; set; }
    }

    public class RubricRatingInputModel
    {
        public int CriterionId { get; set; }

        public int Index { get; set; }

        public double Value { get; set; }

        [MaxLength(80)]
        [Display(Name = "Rating")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
