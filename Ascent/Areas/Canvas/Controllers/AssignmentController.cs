using System.ComponentModel.DataAnnotations;
using Ascent.Areas.Canvas.Models;
using Ascent.Areas.Canvas.Services;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize(AuthenticationSchemes = $"{Constants.AuthenticationScheme.Canvas},{Constants.AuthenticationScheme.Oidc}")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class AssignmentController : Controller
    {
        private readonly CanvasApiService _canvasApiService;
        private readonly CanvasCacheService _canvasCacheService;

        private readonly PersonService _personService;
        private readonly RubricService _rubricService;
        private readonly RubricDataService _rubricDataService;

        private readonly ILogger<AssignmentController> _logger;

        public AssignmentController(CanvasApiService canvasApiService, CanvasCacheService canvasCacheService,
            PersonService personService, RubricService rubricService, RubricDataService rubricDataService,
            ILogger<AssignmentController> logger)
        {
            _canvasApiService = canvasApiService;
            _canvasCacheService = canvasCacheService;
            _personService = personService;
            _rubricService = rubricService;
            _rubricDataService = rubricDataService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ImportRubricAssessmentsAsync(int id, int courseId, bool hasImport = false)
        {
            var assignment = await _canvasCacheService.GetAssignmentAsync(courseId, id);
            var rubrics = _rubricService.GetRubrics();

            var input = new RubricAssessmentsImportInputModel() { DeleteOldImport = hasImport };
            var rubric = rubrics.Where(r => r.Name == assignment.RubricSettings.Title).FirstOrDefault();
            if (rubric != null)
                input.RubricId = rubric.Id;

            ViewBag.Assignment = assignment;
            ViewBag.Rubrics = rubrics;
            ViewBag.Course = await _canvasCacheService.GetCourseAsync(courseId);

            return View(input);
        }

        [HttpPost]
        public async Task<IActionResult> ImportRubricAssessmentsAsync(RubricAssessmentsImportInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            if (input.DeleteOldImport)
                _rubricDataService.DeleteRubricData(input.CanvasAssignmentId.ToString(), RubricDataSourceType.CanvasAssignment);

            var term = new Ascent.Models.Term(input.TermCode);
            var criteria = _rubricService.GetCriteria(input.RubricId);
            var ratingMaps = new Dictionary<int, int>[criteria.Count];
            for (int i = 0; i < criteria.Count; i++)
            {
                ratingMaps[i] = new Dictionary<int, int>();
                foreach (var rating in criteria[i].Ratings)
                    ratingMaps[i][(int)rating.Value] = rating.Id;
            }

            var submissions = await _canvasApiService.GetSubmissions(input.CanvasCourseId, input.CanvasAssignmentId);
            foreach (var submission in submissions)
            {
                var person = _personService.GetPersonByCanvasId(submission.UserId);
                if (person == null) continue;

                for (int i = 0; i < criteria.Count; ++i)
                {
                    var dataPoint = new RubricDataPoint
                    {
                        Year = term.Year,
                        Term = new Ascent.Models.Term(input.TermCode), // see comments in Rubric Data importer
                        CourseId = input.CourseId,
                        AssessmentType = Ascent.Models.RubricAssessmentType.Instructor,
                        EvaluatorId = input.InstructorId,
                        EvaluateeId = person.Id,
                        RubricId = input.RubricId,
                        CriterionId = criteria[i].Id,
                        RatingId = ratingMaps[i][submission.RubricAssessment.Ratings[i].Value],
                        SourceType = RubricDataSourceType.CanvasAssignment,
                        SourceId = input.CanvasAssignmentId.ToString()
                    };

                    if (!string.IsNullOrWhiteSpace(submission.RubricAssessment.Ratings[i].Comments))
                        dataPoint.Comments = submission.RubricAssessment.Ratings[i].Comments;

                    _rubricDataService.AddRubricDataPoint(dataPoint);
                }
            }
            _rubricDataService.SaveChanges();

            _rubricDataService.LogRubricDataImport(new RubricDataImportLogEntry
            {
                RubricId = input.RubricId,
                TermCode = input.TermCode,
                CourseId = input.CourseId,
                SourceType = RubricDataSourceType.CanvasAssignment,
                SourceId = input.CanvasAssignmentId.ToString()
            });

            return RedirectToAction("Section", "Assessment", new
            {
                area = "Rubric",
                rubricId = input.RubricId,
                courseId = input.CourseId,
                termCode = input.TermCode
            });
        }
    }
}

namespace Ascent.Areas.Canvas.Models
{
    public class RubricAssessmentsImportInputModel
    {
        public int CanvasCourseId { get; set; }
        public int CanvasAssignmentId { get; set; }

        [Display(Name = "Rubric")]
        public int RubricId { get; set; }

        public int TermCode { get; set; }

        public int CourseId { get; set; }

        public int InstructorId { get; set; }

        public bool DeleteOldImport { get; set; }
    }
}
