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

        public IActionResult View(int id)
        {
            return View(_courseService.GetCourseJournal(id));
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

            return RedirectToAction("SampleStudents", new { id = courseJournal.Id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult SampleStudents(int id)
        {
            ViewBag.CourseJournal = _courseService.GetCourseJournal(id);
            return View(new SampleStudentInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult AddStudent(SampleStudentInputModel input)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("SampleStudents", new { id = input.CourseJournalId });

            var sampleStudent = _mapper.Map<SampleStudent>(input);
            _courseService.AddSampleStudent(sampleStudent);
            _logger.LogInformation("{user} added sample student {student} to course journal {journal}",
                User.Identity.Name, sampleStudent.Id, sampleStudent.CourseJournalId);

            return RedirectToAction("SampleStudents", new { id = sampleStudent.CourseJournalId });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult RemoveStudent(int id)
        {
            var sampleStudent = _courseService.GetSampleStudent(id);
            if (sampleStudent == null) return NotFound();

            _courseService.RemoveSampleStudent(sampleStudent);
            _logger.LogInformation("{user} removed sample student {student} from course journal {journal}",
                User.Identity.Name, sampleStudent.Id, sampleStudent.CourseJournalId);

            return RedirectToAction("SampleStudents", new { id = sampleStudent.CourseJournalId });
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

    public class SampleStudentInputModel
    {
        public int CourseJournalId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Grade { get; set; }

        [Required, MaxLength(255)]
        public string Url { get; set; }
    }
}
