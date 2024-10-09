using Ascent.Helpers;
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
            _logger.LogInformation("{user} added course template {courseTemplate}", User.GetName(), courseTemplate.Id);

            return RedirectToAction("View", new { id = courseTemplate.Id });
        }

        public IActionResult View(int id)
        {
            return View(_courseTemplateService.GetFullCourseTemplate(id));
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            _courseTemplateService.DeleteCourseTemplate(id);
            _logger.LogInformation("{user} deleted course template {courseTemplate}", User.GetName(), id);
            return RedirectToAction("Index");
        }
    }
}
