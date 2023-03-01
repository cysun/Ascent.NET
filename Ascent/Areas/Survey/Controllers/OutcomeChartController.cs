using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    public class OutcomeChartController : Controller
    {
        private readonly SurveyService _surveyService;
        private readonly ProgramService _programService;
        private readonly SurveyDataService _surveyDataService;

        private readonly IMapper _mapper;
        private readonly ILogger<OutcomeChartController> _logger;

        public OutcomeChartController(SurveyService surveyService, ProgramService programService, SurveyDataService surveyDataService,
            IMapper mapper, ILogger<OutcomeChartController> logger)
        {
            _surveyService = surveyService;
            _programService = programService;
            _surveyDataService = surveyDataService;
            _mapper = mapper;
            _logger = logger;
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

            var dataPoints = _surveyDataService.GetSurveyData(id, selectedYear);
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
    }
}
