using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class CourseJournalController : Controller
    {
        private readonly CourseService _courseService;

        private readonly IMapper _mapper;
        private readonly ILogger<CourseJournalController> _logger;

        public CourseJournalController(CourseService courseService, IMapper mapper, ILogger<CourseJournalController> logger)
        {
            _courseService = courseService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_courseService.GetCourseJournals());
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new CourseJournalInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(CourseJournalInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var courseJournal = _mapper.Map<CourseJournal>(input);
            _courseService.AddCourseJournal(courseJournal);
            _logger.LogInformation("{user} added course journal {journal} for {course}",
                User.Identity.Name, courseJournal.Id, courseJournal.CourseId);

            return RedirectToAction("Index");
        }
    }
}

namespace Ascent.Models
{
    public class CourseJournalInputModel
    {
        public int CourseId { get; set; }

        public int TermCode { get; set; }

        public int InstructorId { get; set; }

        [Required, MaxLength(255), Display(Name = "Course URL")]
        public string CourseUrl { get; set; }

        [Required, MaxLength(255), Display(Name = "Syllabus URL")]
        public string SyllabusUrl { get; set; }
    }
}
