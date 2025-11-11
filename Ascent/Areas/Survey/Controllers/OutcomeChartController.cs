using Ascent.Models;
using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    public class OutcomeChartController : Controller
    {
        private readonly ProgramService _programService;
        private readonly SurveyDataService _surveyDataService;

        public OutcomeChartController(ProgramService programService, SurveyDataService surveyDataService)
        {
            _programService = programService;
            _surveyDataService = surveyDataService;
        }

        public IActionResult Index()
        {
            return View(_programService.GetPrograms());
        }

        public IActionResult Program(int id, int? year)
        {
            var program = _programService.GetProgram(id);
            if (program == null) return NotFound();

            var years = _surveyDataService.GetSurveyYears(id);
            if (years.Count == 0) return NotFound();

            var selectedYear = year ?? years.Last();

            var outcomeIdToIndex = new Dictionary<int, int>();
            for (int i = 0; i < program.Outcomes.Count; ++i)
                outcomeIdToIndex[program.Outcomes[i].Id] = i;

            var dataPoints = _surveyDataService.GetSurveyDataByProgram(id, selectedYear);
            var dataTables = new Dictionary<ConstituencyType, object[][]>();
            foreach (var dataPoint in dataPoints)
            {
                var constituencyType = dataPoint.ConstituencyType;
                if (!dataTables.ContainsKey(constituencyType))
                    dataTables[constituencyType] = createDataTable(program.Outcomes);

                var outcomeIndex = outcomeIdToIndex[dataPoint.OutcomeId];
                dataTables[constituencyType][outcomeIndex][dataPoint.Value] =
                    (int)dataTables[constituencyType][outcomeIndex][dataPoint.Value] + 1;
            }

            ViewBag.Program = program;
            ViewBag.Years = years;
            ViewBag.Year = selectedYear;

            return View(dataTables);
        }

        // Each data table has outcomes.Count rows. In each row, the first column is the outcome index,
        // followed by the count of each rating (1-5 or from "Strongly Disagree" to "Strongly Agree").
        private object[][] createDataTable(List<ProgramOutcome> outcomes)
        {
            var dataTable = new object[outcomes.Count][];
            for (int i = 0; i < outcomes.Count; i++)
            {
                var row = new object[6];
                row[0] = (i + 1).ToString();
                for (int j = 1; j < row.Length; j++)
                    row[j] = 0;
                dataTable[i] = row;
            }
            return dataTable;
        }

        public IActionResult Outcome(int id, int? fromYear, int? toYear)
        {
            var outcome = _programService.GetProgramOutcome(id);
            if (outcome == null) return NotFound();

            var years = _surveyDataService.GetSurveyYears(outcome.Program.Id);
            if (years.Count == 0) return NotFound();

            var _fromYear = fromYear ?? (years.Count >= 6 ? years[years.Count - 6] : years.First());
            var _toYear = toYear ?? years.Last();
            var _years = years.Where(y => _fromYear <= y && y <= _toYear).ToList();
            var yearToIndex = new Dictionary<int, int>();
            for (int i = 0; i < _years.Count; ++i)
                yearToIndex[_years[i]] = i;

            var dataPoints = _surveyDataService.GetSurveyDataByOutcome(id, _fromYear, _toYear);
            var dataTables = new Dictionary<ConstituencyType, object[][]>();

            foreach (var dataPoint in dataPoints)
            {
                var constituencyType = dataPoint.ConstituencyType;
                if (!dataTables.ContainsKey(constituencyType))
                    dataTables[constituencyType] = createDataTable(_years);

                var yearIndex = yearToIndex[dataPoint.Year];
                dataTables[constituencyType][yearIndex][dataPoint.Value] =
                    (int)dataTables[constituencyType][yearIndex][dataPoint.Value] + 1;
            }

            ViewBag.Outcome = outcome;
            ViewBag.Years = years;
            ViewBag.FromYear = _fromYear;
            ViewBag.ToYear = _toYear;

            return View(dataTables);
        }

        // Each data table has years.Count rows. In each row, the first column is the year, followed
        // by the count of each rating (1-5 or from "Strongly Disagree" to "Strongly Agree").
        private object[][] createDataTable(List<int> years)
        {
            var dataTable = new object[years.Count][];
            for (int i = 0; i < years.Count; i++)
            {
                var row = new object[6];
                row[0] = years[i].ToString();
                for (int j = 1; j < row.Length; j++)
                    row[j] = 0;
                dataTable[i] = row;
            }
            return dataTable;
        }
    }
}
