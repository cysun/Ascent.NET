using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class AssignmentTemplateController : Controller
    {
        private readonly CourseTemplateService _courseTemplateService;
        private readonly AssignmentTemplateService _assignmentTemplateService;
        private readonly RubricService _rubricService;

        private readonly AppMapper _mapper;
        private readonly ILogger<AssignmentTemplateController> _logger;

        public AssignmentTemplateController(CourseTemplateService courseTemplateService, AssignmentTemplateService assignmentTemplateSerivce,
            RubricService rubricService, AppMapper mapper, ILogger<AssignmentTemplateController> logger)
        {
            _courseTemplateService = courseTemplateService;
            _assignmentTemplateService = assignmentTemplateSerivce;
            _rubricService = rubricService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Add(int courseTemplateId)
        {
            ViewBag.CourseTemplate = _courseTemplateService.GetCourseTemplate(courseTemplateId);
            ViewBag.Rubrics = _rubricService.GetRubrics();
            return View(new AssignmentTemplateInputModel());
        }

        [HttpPost]
        public IActionResult Add(AssignmentTemplateInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var assignmentTemplate = _mapper.Map(input);
            _assignmentTemplateService.AddAssignmentTemplate(assignmentTemplate);
            _logger.LogInformation("{user} added assignment template {assignmentTemplate}", User.GetName(), assignmentTemplate.Id);

            return RedirectToAction("View", "CourseTemplate", new { id = assignmentTemplate.CourseTemplateId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var assignmentTemplate = _assignmentTemplateService.GetAssignmentTemplate(id);
            if (assignmentTemplate == null) return NotFound();

            ViewBag.AssignmentTemplate = assignmentTemplate;
            ViewBag.CourseTemplate = _courseTemplateService.GetCourseTemplate(assignmentTemplate.CourseTemplateId);
            ViewBag.Rubrics = _rubricService.GetRubrics();

            return View(_mapper.Map(assignmentTemplate));
        }

        [HttpPost]
        public IActionResult Edit(int id, AssignmentTemplateInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var assignmentTemplate = _assignmentTemplateService.GetAssignmentTemplate(id);
            if (assignmentTemplate == null) return NotFound();

            _mapper.Map(input, assignmentTemplate);
            _assignmentTemplateService.SaveChanges();
            _logger.LogInformation("{user} edited assignment template {assignmentTemplate}", User.GetName(), id);

            return RedirectToAction("View", "CourseTemplate", new { id = assignmentTemplate.CourseTemplateId });
        }

        public IActionResult Delete(int id)
        {
            var assignmentTemplate = _assignmentTemplateService.GetAssignmentTemplate(id);
            if (assignmentTemplate == null) return NotFound();

            _assignmentTemplateService.DeleteAssignmentTemplate(assignmentTemplate);
            _logger.LogInformation("{user} deleted assignment template {assignmentTemplate}", User.GetName(), id);
            return RedirectToAction("View", "CourseTemplate", new { id = assignmentTemplate.CourseTemplateId });
        }
    }
}

namespace Ascent.Models
{
    public class AssignmentTemplateInputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int CourseTemplateId { get; set; }

        [Display(Name = "Rubric")]
        public int? RubricId { get; set; }

        [Display(Name = "Peer Review")]
        public bool IsPeerReviewed { get; set; }
    }
}
