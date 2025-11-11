using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class CourseJournalController : Controller
    {
        private readonly CourseService _courseService;
        private readonly CourseJournalService _courseJournalService;

        private readonly AppMapper _mapper;
        private readonly ILogger<CourseJournalController> _logger;

        public CourseJournalController(CourseService courseService, CourseJournalService courseJournalService,
            AppMapper mapper, ILogger<CourseJournalController> logger)
        {
            _courseService = courseService;
            _courseJournalService = courseJournalService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_courseService.GetJournalCourses());
        }

        public IActionResult View(int id)
        {
            var courseJournals = _courseJournalService.GetCourseJournals();
            var courseJournal = courseJournals.Where(j => j.Id == id).FirstOrDefault();
            if (courseJournal == null) return NotFound();

            ViewBag.CourseJournals = courseJournals;
            return View(courseJournal);
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

            var courseJournal = _mapper.Map(input);
            courseJournal = _courseJournalService.AddOrUpdateCourseJournal(courseJournal);
            _logger.LogInformation("{user} added/updated course journal {journal} for {course}",
                User.GetName(), courseJournal.Id, courseJournal.CourseId);

            return RedirectToAction("View", new { id = courseJournal.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var courseJournal = _courseJournalService.GetCourseJournal(id);
            if (courseJournal == null) return NotFound();

            ViewBag.CourseJournal = courseJournal;
            return View(_mapper.Map(courseJournal));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, CourseJournalInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var courseJournal = _courseJournalService.GetCourseJournal(id);
            if (courseJournal == null) return NotFound();

            _mapper.Map(input, courseJournal);
            _courseJournalService.SaveChanges();
            _logger.LogInformation("{user} edited course journal {courseJournal}", User.GetName(), id);

            return RedirectToAction("View", new { id });
        }

        public IActionResult Delete(int id)
        {
            var courseJournal = _courseJournalService.GetCourseJournal(id);
            if (courseJournal == null) return NotFound();

            _courseJournalService.DeleteCourseJournal(courseJournal);
            _logger.LogInformation("{user} deleted course journal {journal} for {course}",
                User.GetName(), courseJournal.Id, courseJournal.CourseId);

            return RedirectToAction("Index");
        }
    }
}

namespace Ascent.Models
{
    public class CourseJournalInputModel
    {
        public int CourseId { get; set; }

        [Display(Name = "Term")]
        public int TermCode { get; set; }

        public int InstructorId { get; set; }

        [Required, MaxLength(255), Display(Name = "Course URL")]
        public string CourseUrl { get; set; }

        [MaxLength(255), Display(Name = "Sample Student Work URL")]
        public string SampleStudentWorkUrl { get; set; }
    }
}
