using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class CourseController : Controller
    {
        private readonly CourseService _courseService;

        private readonly IMapper _mapper;
        private readonly ILogger<CourseController> _logger;

        public CourseController(CourseService courseService, IMapper mapper, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var courses = _courseService.GetCourses();
            ViewBag.UndergraduateCourses = courses.Where(c => !c.IsGraduateCourse).ToList();
            ViewBag.GraduateCourses = courses.Where(c => c.IsGraduateCourse).ToList();

            return View();
        }
    }
}
