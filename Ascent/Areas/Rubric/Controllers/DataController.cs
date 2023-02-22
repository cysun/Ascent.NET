using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Rubric.Controllers
{
    [Area("Rubric")]
    public class DataController : Controller
    {
        private readonly RubricService _rubricService;
        private readonly RubricDataService _rubricDataService;

        public DataController(RubricService rubricService, RubricDataService rubricDataService)
        {
            _rubricService = rubricService;
            _rubricDataService = rubricDataService;
        }

        public IActionResult Index(int rubricId)
        {
            var rubric = _rubricService.GetRubric(rubricId);
            if (rubric == null) return NotFound();

            ViewBag.Rubric = rubric;
            var sections = _rubricDataService.GetAssessmentSections(rubricId);
            var model = sections.GroupBy(s => s.Course)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.TermCode).ToList());

            return View(model);
        }
    }
}
