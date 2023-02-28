using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class OutcomeSurveyController : Controller
    {
        private readonly SurveyService _surveyService;
        private readonly ProgramService _programService;

        private readonly IMapper _mapper;
        private readonly ILogger<OutcomeSurveyController> _logger;

        public OutcomeSurveyController(SurveyService surveyService, ProgramService programService,
            IMapper mapper, ILogger<OutcomeSurveyController> logger)
        {
            _surveyService = surveyService;
            _programService = programService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var surveys = _surveyService.GetSurveys();
            var outcomeSurveys = _surveyService.GetOutcomeSurveys();
            var outcomeSurveyIds = _surveyService.GetOutcomeSurveys().Select(s => s.SurveyId).ToHashSet();

            ViewBag.ShowAdd = surveys.Where(s => !outcomeSurveyIds.Contains(s.Id) && s.Name.Contains("Outcome")).Count() > 0;

            return View(outcomeSurveys);
        }

        public IActionResult View(int id)
        {
            var outcomeSurvey = _surveyService.GetOutcomeSurveyWithProgram(id);

            ViewBag.Questions = _surveyService.GetQuestions(outcomeSurvey.SurveyId);

            return View(outcomeSurvey);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var surveys = _surveyService.GetSurveys();
            var outcomeSurveyIds = _surveyService.GetOutcomeSurveys().Select(s => s.SurveyId).ToHashSet();

            ViewBag.Programs = _programService.GetPrograms();
            ViewBag.Surveys = surveys.Where(s => !outcomeSurveyIds.Contains(s.Id) && s.Name.Contains("Outcome")).ToList();

            return View(new OutcomeSurveyInputModel());
        }

        [HttpPost]
        public IActionResult Add(OutcomeSurveyInputModel input)
        {
            var outcomeSurvey = _mapper.Map<OutcomeSurvey>(input);

            var program = _programService.GetProgram(input.ProgramId);
            outcomeSurvey.QuestionIds = new int[program.Outcomes.Count];

            var questions = _surveyService.GetQuestions(input.SurveyId);
            for (int i = 0; i < outcomeSurvey.QuestionIds.Length; ++i)
                outcomeSurvey.QuestionIds[i] = questions[i].Id;

            _surveyService.AddOutcomeSurvey(outcomeSurvey);
            _logger.LogInformation("{user} created outcome survey {survey}", User.Identity.Name, outcomeSurvey.Id);

            return RedirectToAction("Index");
        }
    }
}

namespace Ascent.Models
{
    public class OutcomeSurveyInputModel
    {
        [Display(Name = "Constituency Type")]
        public ConstituencyType ConstituencyType { get; set; }

        [Display(Name = "Survey")]
        public int SurveyId { get; set; }

        [Display(Name = "Program")]
        public int ProgramId { get; set; }

        public int[] QuestionIds { get; set; }
    }
}
