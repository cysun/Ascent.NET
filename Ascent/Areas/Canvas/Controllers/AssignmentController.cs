using System.ComponentModel.DataAnnotations;
using Ascent.Areas.Canvas.Models;
using Ascent.Areas.Canvas.Services;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel;
using RubricAssessmentType = Ascent.Models.RubricAssessmentType;
using Term = Ascent.Models.Term;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize(Policy = Constants.Policy.HasCat)]
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
            var rubric = rubrics.FirstOrDefault(r => r.Name == assignment.RubricSettings.Title);
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
                _rubricDataService.DeleteRubricData(input.CanvasAssignmentId.ToString());

            var term = new Term(input.TermCode);
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
                    var ratingValue = submission.RubricAssessment?.Ratings[i]?.Value;
                    if (ratingValue == null || !ratingMaps[i].ContainsKey((int)ratingValue))
                    {
                        _logger.LogWarning("Invalid/incomplete rating value: {rating}", ratingValue);
                        continue;
                    }

                    var dataPoint = new RubricDataPoint
                    {
                        Year = term.Year,
                        Term = new Term(input.TermCode), // see comments in Rubric Data importer
                        CourseId = input.CourseId,
                        AssessmentType = RubricAssessmentType.Instructor,
                        EvaluatorId = input.InstructorId,
                        EvaluateeId = person.Id,
                        RubricId = input.RubricId,
                        CriterionId = criteria[i].Id,
                        RatingId = ratingMaps[i][(int)ratingValue],
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

        public async Task<IActionResult> DownloadRubricAssessmentsAsync(int id, int courseId)
        {
            var submissions = await _canvasApiService.GetSubmissions(courseId, id);
            if (!submissions.Any())
                return NotFound();

            var n = submissions[0].RubricAssessment.Ratings.Length;
            string[] cols = { "Student", "CIN" };
            cols = cols.Concat(Enumerable.Range(1, n).Select(i => $"Criterion {i}")).Concat(["Comments"]).ToArray();

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Rubric Assessments");
            var row = sheet.CreateRow(0);
            for (var i = 0; i < cols.Length; i++) row.CreateCell(i).SetCellValue(cols[i]);

            for (var i = 0; i < submissions.Count; i++)
            {
                var submission = submissions[i];
                if (submission.RubricAssessment == null)
                {
                    _logger.LogWarning("No rubric assessment found for submission Id {submissionId}", submission.Id);
                    continue;
                }

                var person = _personService.GetPersonByCanvasId(submission.UserId);
                if (person == null)
                {
                    _logger.LogWarning("Can't find person with Canvas Id {personId}", submission.UserId);
                    continue;
                }

                row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(person.FullName2);
                row.CreateCell(1).SetCellValue(person.CampusId);
                for (var j = 0; j < n; j++)
                {
                    var rating = submission.RubricAssessment.Ratings[j];
                    row.CreateCell(j + 2).SetCellValue(rating?.Value.ToString());
                }

                row.CreateCell(n + 2).SetCellValue(string.Join("\n", submission.RubricAssessment.Ratings
                    .Where(r => !string.IsNullOrWhiteSpace(r?.Comments))
                    .Select(r => r.Comments)));
            }

            using var stream = new MemoryStream();
            workbook.Write(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Rubric Assessments.xlsx");
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
