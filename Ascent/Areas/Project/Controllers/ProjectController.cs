using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Project.Controllers
{
    [Area("Project")]
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;

        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ProjectService projectService, ILogger<ProjectController> logger)
        {
            _projectService = projectService;
            _logger = logger;
        }

        public IActionResult Index(string year)
        {
            var academicYears = _projectService.GetAcademicYears();
            var academicYear = year != null ? year : academicYears.FirstOrDefault();

            ViewBag.AcademicYears = academicYears;
            ViewBag.AcademicYear = academicYear;

            return View(_projectService.GetProjects(academicYear));
        }

        public IActionResult View(int id)
        {
            return View(_projectService.GetProject(id));
        }
    }
}
