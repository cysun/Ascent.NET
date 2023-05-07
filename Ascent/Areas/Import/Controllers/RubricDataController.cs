using System.ComponentModel.DataAnnotations;
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
    public class RubricDataController : Controller
    {

        private readonly PersonService _personService;
        private readonly CourseService _courseService;
        private readonly RubricService _rubricService;
        private readonly RubricDataService _rubricDataService;
        private readonly ILogger<RubricDataController> _logger;

        public RubricDataController(PersonService personService, CourseService courseService, RubricService rubricService,
            RubricDataService rubricDataService, ILogger<RubricDataController> logger)
        {
            _personService = personService;
            _courseService = courseService;
            _rubricService = rubricService;
            _rubricDataService = rubricDataService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Import()
        {
            ViewBag.Rubrics = _rubricService.GetRubrics();
            return View(new RubricDataImportInputModel());
        }

        [HttpPost]
        public IActionResult Import(RubricDataImportInputModel input, IFormFile uploadedFile)
        {
            if (!ModelState.IsValid) return View(input);

            var term = new Term(input.TermCode);
            var criteria = _rubricService.GetCriteria(input.RubricId);
            var ratingMaps = new Dictionary<string, int>[criteria.Count];
            for (int i = 0; i < criteria.Count; i++)
            {
                ratingMaps[i] = new Dictionary<string, int>();
                foreach (var rating in criteria[i].Ratings)
                    ratingMaps[i][rating.Value.ToString()] = rating.Id;
            }

            var excelReader = new ExcelReader(uploadedFile.OpenReadStream());
            while (excelReader.Next())
            {
                var person = GetOrCreatePerson(excelReader);
                for (int i = 0; i < criteria.Count; ++i)
                {
                    _rubricDataService.AddRubricDataPoint(new RubricDataPoint
                    {
                        Year = term.Year,
                        Term = new Term(input.TermCode), // use term here seems to cause all but one data point to have null TermCode
                        CourseId = input.CourseId,
                        AssessmentType = RubricAssessmentType.Instructor,
                        EvaluatorId = input.InstructorId,
                        EvaluateeId = person.Id,
                        RubricId = input.RubricId,
                        CriterionId = criteria[i].Id,
                        RatingId = ratingMaps[i][excelReader.Get($"C{i+1}")]
                    });
                }
            }
            _rubricDataService.SaveChanges();

            return RedirectToAction("Index", "Assessment", new { Area = "Rubric", rubricId = input.RubricId });
        }

        private Person GetOrCreatePerson(ExcelReader excelReader)
        {
            var cin = excelReader.Get("CIN");
            var person = _personService.GetPersonByCampusId(cin);
            if (person == null)
            {
                var (firstName, lastName) = Utils.SplitName(excelReader.Get("Name"));
                person = new Person
                {
                    CampusId = cin,
                    FirstName = firstName,
                    LastName = lastName
                };
                _personService.AddPerson(person);
                _logger.LogInformation("New person [{cin}, {firstName} {lastName}] created during import.", cin, firstName, lastName);
            }

            return person;
        }
    }
}

namespace Ascent.Models
{
    public class RubricDataImportInputModel
    {
        [Display(Name = "Rubric")]
        public int RubricId { get; set; }

        public int TermCode { get; set; }

        public int CourseId { get; set; }

        public int InstructorId { get; set; }
    }
}
