using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class CourseController : Controller
    {
        private const string AbetSyllabusFolder = "/ABET Course Syllabi";

        private readonly CourseService _courseService;
        private readonly FileService _fileService;

        private readonly IMapper _mapper;
        private readonly ILogger<CourseController> _logger;

        public CourseController(CourseService courseService, FileService fileService, IMapper mapper, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _fileService = fileService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var courses = _courseService.GetCourses();
            ViewBag.UndergraduateCourses = courses.Where(c => !c.IsGraduateCourse).ToList();
            ViewBag.GraduateCourses = courses.Where(c => c.IsGraduateCourse).ToList();

            return View();
        }

        [AllowAnonymous]
        public IActionResult View(int id)
        {
            var course = _courseService.GetCourse(id);
            if (course == null) return NotFound();

            ViewBag.Courses = _courseService.GetCourses().Where(c => c.IsGraduateCourse == course.IsGraduateCourse).ToList();
            return View(course);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new CourseInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public async Task<IActionResult> AddAsync(CourseInputModel input, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid) return View(input);

            var course = _mapper.Map<Course>(input);

            if (uploadedFile != null)
            {
                var folder = _fileService.GetFolder(AbetSyllabusFolder, true, false);
                course.AbetSyllabus = await _fileService.UploadFileAsync(folder.Id, uploadedFile, false, $"{course.Code} ABET Syllabus");
            }

            _courseService.AddCourse(course);
            _logger.LogInformation("{user} added course {course}", User.GetName(), course.Code);

            return RedirectToAction("View", new { id = course.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var course = _courseService.GetCourse(id);
            if (course == null) return NotFound();

            ViewBag.Course = course;

            return View(_mapper.Map<CourseInputModel>(course));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public async Task<IActionResult> EditAsync(int id, CourseInputModel input, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid) return View(input);

            var course = _courseService.GetCourse(id);
            if (course == null) return NotFound();

            _mapper.Map(input, course);

            if (uploadedFile != null)
            {
                var folder = _fileService.GetFolder(AbetSyllabusFolder, true, false);
                course.AbetSyllabus = await _fileService.UploadFileAsync(folder.Id, uploadedFile, false, $"{course.Code} ABET Syllabus");
            }

            _courseService.SaveChanges();
            _logger.LogInformation("{user} edited course {course}", User.GetName(), course.Code);

            return RedirectToAction("View", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var course = _courseService.GetCourse(id);
            if (course == null) return NotFound();

            course.IsObsolete = true;
            _courseService.SaveChanges();
            _logger.LogInformation("{user} deleted course {course}", User.GetName(), id);

            return RedirectToAction("Index");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Coordinators(int id)
        {
            var course = _courseService.GetCourse(id);
            if (course == null) return NotFound();

            return View(course);
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult AddCoordinator(int id, int personId)
        {
            _courseService.AddCoordinator(id, personId);
            _logger.LogInformation("{user} added coordinator {coordinator} to course {course}", User.GetName(), personId, id);

            return RedirectToAction("Coordinators", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult RemoveCoordinator(int id, int personId)
        {
            _courseService.RemoveCoordinator(id, personId);
            _logger.LogInformation("{user} removed coordinator {coordinator} from course {course}", User.GetName(), personId, id);

            return RedirectToAction("Coordinators", new { id });
        }

        public List<Course> Autocomplete(string prefix)
        {
            return _courseService.SearchCoursesByPrefix(prefix);
        }
    }
}

namespace Ascent.Models
{
    public class CourseInputModel
    {
        [Required, MaxLength(6)]
        public string Subject { get; set; } = "CS";

        [Required, MaxLength(6)]
        public string Number { get; set; } // e.g. 3220

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Display(Name = "Min Units")]
        public int MinUnits { get; set; } = 3;

        [Display(Name = "Max Units")]
        public int MaxUnits { get; set; } = 3;

        [Display(Name = "Catalog Description")]
        public string CatalogDescription { get; set; }

        [Display(Name = "Required Course")]
        public bool IsRequired { get; set; }

        [Display(Name = "Elective Course")]
        public bool IsElective { get; set; }

        [Display(Name = "Service Course")]
        public bool IsService { get; set; }

        [Display(Name = "Obsolete")]
        public bool IsObsolete { get; set; }
    }
}
