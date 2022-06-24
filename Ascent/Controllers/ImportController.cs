using Ascent.Models;
using Ascent.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class ImportController : Controller
    {
        private readonly PersonService _personService;
        private readonly SectionService _sectionService;
        private readonly EnrollmentService _enrollmentService;

        private readonly ILogger<ImportController> _logger;

        public ImportController(PersonService personService, SectionService sectionService,
            EnrollmentService enrollmentService, ILogger<ImportController> logger)
        {
            _personService = personService;
            _sectionService = sectionService;
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Grades(int? termCode)
        {
            var terms = new List<Term>();
            if (termCode == null)
            {
                var currentTerm = new Term();
                for (int i = 1; i < 10; ++i)
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
        public IActionResult Grades(int termCode, int courseId, int instructorId, IFormFile uploadedFile)
        {
            Section section = new Section()
            {
                Term = new Term(termCode),
                CourseId = courseId,
                InstructorId = instructorId
            };
            _sectionService.AddSection(section);
            _logger.LogInformation("{user} created section {section} for grade import", User.Identity.Name, section.Id);

            var htmlDoc = new HtmlDocument();
            htmlDoc.Load(uploadedFile.OpenReadStream());

            var colLabels = new Dictionary<int, string>();
            var rows = htmlDoc.DocumentNode.SelectNodes("//tr");

            var rowIndex = 1;
            foreach (var row in rows)
            {
                int colIndex = 1;

                if (rowIndex == 1)
                {
                    foreach (var col in row.SelectNodes(".//th"))
                        colLabels.Add(colIndex++, col.InnerText);
                    ++rowIndex;
                    continue;
                }

                var record = new ImportedGradeRecord();
                foreach (var col in row.SelectNodes(".//td"))
                {
                    switch (colLabels[colIndex++])
                    {
                        case "ID":
                            record.CampusId = col.InnerText;
                            break;
                        case "Name":
                            record.Name = col.InnerText;
                            break;
                        case "Official Grade":
                            record.GradeSymbol = col.InnerText;
                            break;
                    }
                }

                var student = GetOrCreateStudent(record);
                var enrollment = new Enrollment()
                {
                    SectionId = section.Id,
                    StudentId = student.Id,
                    GradeSymbol = record.GradeSymbol
                };
                _enrollmentService.AddEnrollment(enrollment);

                ++rowIndex;
            }

            _logger.LogInformation("{user} imported grades of {students} students", User.Identity.Name, rowIndex - 2);

            return RedirectToAction("View", "Section", new { id = section.Id });
        }

        public Person GetOrCreateStudent(ImportedGradeRecord record)
        {
            var student = _personService.GetPersonByCampusId(record.CampusId);
            if (student == null)
                _personService.AddPerson(new Person()
                {
                    CampusId = record.CampusId,
                    FirstName = record.FirstName,
                    LastName = record.LastName
                });
            return student;
        }

        public class ImportedGradeRecord
        {
            public string CampusId { get; set; }

            private string _firstName, _lastName;

            public string FirstName => _firstName;
            public string LastName => _lastName;

            public string Name
            {
                set
                {
                    string[] tokens = value.Split(',');
                    _firstName = tokens[0];
                    _lastName = tokens[1];
                }
            }

            public string GradeSymbol { get; set; }
        }
    }
}
