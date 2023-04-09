using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    public class CourseTemplateController : Controller
    {
        private readonly CourseTemplateService _courseTemplateService;

        private readonly ILogger<CourseTemplateController> _logger;

        public CourseTemplateController(CourseTemplateService courseTemplateService, ILogger<CourseTemplateController> logger)
        {
            _courseTemplateService = courseTemplateService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_courseTemplateService.GetCourseTemplates());
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(int courseId)
        {
            if (_courseTemplateService.IsCourseTemplateExists(courseId))
                return RedirectToAction("Index");

            var courseTemplate = new CourseTemplate
            {
                CourseId = courseId
            };
            _courseTemplateService.AddCourseTemplate(courseTemplate);
            _logger.LogInformation("{user} added course template {courseTemplate}", User.Identity.Name, courseTemplate.Id);

            return RedirectToAction("Index");
        }

        public IActionResult View(int id)
        {
            return View(_courseTemplateService.GetCourseTemplate(id));
        }
    }
}
