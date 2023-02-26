using Ascent.Models;
using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Rubric.Controllers
{
    [Area("Rubric")]
    public class AssessmentController : Controller
    {
        private readonly CourseService _courseService;
        private readonly RubricService _rubricService;
        private readonly RubricDataService _rubricDataService;

        public AssessmentController(CourseService courseService, RubricService rubricService, RubricDataService rubricDataService)
        {
            _courseService = courseService;
            _rubricService = rubricService;
            _rubricDataService = rubricDataService;
        }

        public IActionResult Index(int rubricId)
        {
            var rubric = _rubricService.GetRubric(rubricId);
            if (rubric == null) return NotFound();

            ViewBag.Rubric = rubric;
            var sections = _rubricDataService.GetAssessmentSections(rubricId);
            var model = sections.GroupBy(s => s.Course)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.TermCode).ToList());

            return View(model);
        }

        public IActionResult Section(int rubricId, int courseId, int termCode)
        {
            var rubric = _rubricService.GetRubric(rubricId);
            if (rubric == null) return NotFound();

            var course = _courseService.GetCourse(courseId);
            if (course == null) return NotFound();

            var criteria = _rubricService.GetCriteria(rubricId);
            if (criteria.Count == 0) return NotFound();

            var criterionIdToIndex = new Dictionary<int, int>();
            for (int i = 0; i < criteria.Count; ++i)
                criterionIdToIndex[criteria[i].Id] = i;

            var dataTables = new Dictionary<RubricAssessmentType, object[][]>();
            var dataByPerson = _rubricDataService.GetDataByPerson(rubricId, courseId, termCode);
            foreach (var dataEntry in dataByPerson)
            {
                var assessmentType = dataEntry.AssessmentType;
                if (!dataTables.ContainsKey(assessmentType))
                    dataTables[assessmentType] = createDataTable(criteria);

                var criterionIndex = criterionIdToIndex[dataEntry.CriterionId];
                var ratingIndex = criteria[criterionIndex].ValueToRatingIndex(dataEntry.AvgRatingValue);
                dataTables[assessmentType][criterionIndex][ratingIndex + 1] =
                    (int)dataTables[assessmentType][criterionIndex][ratingIndex + 1] + 1;
            }

            ViewBag.Rubric = rubric;
            ViewBag.Course = course;
            ViewBag.Term = new Term(termCode);

            return View(dataTables);
        }

        // Each data table has criteria.Count rows. In each row, the first column is the criterion
        // name, followed by the count of each rating.
        private object[][] createDataTable(List<RubricCriterion> criteria)
        {
            var dataTable = new object[criteria.Count][];
            for (int i = 0; i < criteria.Count; i++)
            {
                var row = new object[criteria[i].Ratings.Count + 1];
                row[0] = criteria[i].Name;
                for (int j = 0; j < criteria[i].Ratings.Count; j++)
                    row[j + 1] = 0;
                dataTable[i] = row;
            }
            return dataTable;
        }

        public IActionResult Course(int courseId, int rubricId, int? fromYear, int? toYear)
        {
            var rubric = _rubricService.GetRubric(rubricId);
            if (rubric == null) return NotFound();

            var course = _courseService.GetCourse(courseId);
            if (course == null) return NotFound();

            var criteria = _rubricService.GetCriteria(rubricId);
            if (criteria.Count == 0) return NotFound();

            var criterionIdToIndex = new Dictionary<int, int>();
            for (int i = 0; i < criteria.Count; ++i)
                criterionIdToIndex[criteria[i].Id] = i;

            var years = _rubricDataService.GetAssessmentYears(rubricId, courseId);
            var _fromYear = fromYear ?? (years.Count >= 6 ? years[years.Count - 6] : years.First());
            var _toYear = toYear ?? years.Last();

            var dataTables = new Dictionary<RubricAssessmentType, object[][]>();
            var dataByPerson = _rubricDataService.GetDataByPerson(rubricId, courseId, _fromYear, _toYear);
            var selectedYears = years.Where(y => _fromYear <= y && y <= _toYear).ToList();
            foreach (var dataEntry in dataByPerson)
            {
                var assessmentType = dataEntry.AssessmentType;
                if (!dataTables.ContainsKey(assessmentType))
                    dataTables[assessmentType] = createDataTable(selectedYears, criteria);

                var yearIndex = selectedYears.IndexOf(dataEntry.Year);
                var criterionIndex = criterionIdToIndex[dataEntry.CriterionId];
                var ratingIndex = criteria[criterionIndex].ValueToRatingIndex(dataEntry.AvgRatingValue);
                dataTables[assessmentType][yearIndex * criteria.Count + criterionIndex][ratingIndex + 2] =
                    (int)dataTables[assessmentType][yearIndex * criteria.Count + criterionIndex][ratingIndex + 2] + 1;
            }

            ViewBag.Rubric = rubric;
            ViewBag.Course = course;
            ViewBag.Criteria = criteria;
            ViewBag.FromYear = _fromYear;
            ViewBag.ToYear = _toYear;
            ViewBag.Years = years;

            return View(dataTables);
        }

        // Each data table has years*criteria.Count rows. In each row, the first column is the year,
        // the second column is the criterion name, followed by the count of each rating.
        private object[][] createDataTable(List<int> years, List<RubricCriterion> criteria)
        {
            var dataTable = new object[years.Count * criteria.Count][];
            for (int i = 0; i < years.Count; ++i)
            {
                for (int j = 0; j < criteria.Count; j++)
                {
                    var row = new object[criteria[j].Ratings.Count + 2];
                    row[0] = years[i].ToString();
                    row[1] = criteria[j].Name;
                    for (int k = 0; k < criteria[j].Ratings.Count; k++)
                        row[k + 2] = 0;
                    dataTable[i * criteria.Count + j] = row;
                }
            }

            return dataTable;
        }
    }
}
