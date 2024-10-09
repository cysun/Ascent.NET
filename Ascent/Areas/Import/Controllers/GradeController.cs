using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Import.Controllers
{
    [Area("Import")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class GradeController : Controller
    {
        private readonly PersonService _personService;
        private readonly SectionService _sectionService;
        private readonly EnrollmentService _enrollmentService;

        private readonly ILogger<GradeController> _logger;

        public GradeController(PersonService personService, SectionService sectionService,
            EnrollmentService enrollmentService, ILogger<GradeController> logger)
        {
            _personService = personService;
            _sectionService = sectionService;
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Import(int? termCode)
        {
            var terms = new List<Term>();
            if (termCode == null)
            {
                var currentTerm = new Term();
                for (var i = 1; i < 10; ++i)
                {
                    terms.Add(currentTerm);
                    currentTerm = currentTerm.Previous();
                }
            }
            else
            {
                terms.Add(new Term((int)termCode));
            }

            ViewBag.Terms = terms;

            return View();
        }

        [HttpPost]
        public IActionResult Import(int termCode, int courseId, int instructorId, IFormFile uploadedFile)
        {
            var section = new Section()
            {
                Term = new Term(termCode),
                CourseId = courseId,
                InstructorId = instructorId
            };
            _sectionService.AddSection(section);
            _logger.LogInformation("{user} created section {section} for grade import", User.GetName(), section.Id);

            var excelReader = new ExcelReader(uploadedFile.OpenReadStream());
            while (excelReader.Next())
            {
                var person = GetOrCreatePerson(excelReader);
                var enrollment = new Enrollment()
                {
                    SectionId = section.Id,
                    StudentId = person.Id,
                    GradeSymbol = excelReader.Get("Official Grade")
                };
                _enrollmentService.AddEnrollment(enrollment);
            }

            return RedirectToAction("View", "Section", new { Area = "", id = section.Id });
        }

        private Person GetOrCreatePerson(ExcelReader excelReader)
        {
            var cin = excelReader.Get("ID");
            var person = _personService.GetPersonByCampusId(cin);
            if (person == null)
            {
                var tokens = excelReader.Get("Name").Split(',', StringSplitOptions.TrimEntries);
                var midIndex = tokens[1].IndexOf(' ');
                person = new Person
                {
                    CampusId = cin,
                    LastName = tokens[0],
                    FirstName = midIndex > 0 ? tokens[1].Substring(0, midIndex) : tokens[1],
                    MiddleName = midIndex > 0 ? tokens[1].Substring(midIndex + 1) : null
                };
                _personService.AddPerson(person);
            }
            return person;
        }
    }
}
