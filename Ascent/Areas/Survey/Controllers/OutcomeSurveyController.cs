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
        private readonly SurveyDataService _surveyDataService;

        private readonly IMapper _mapper;
        private readonly ILogger<OutcomeSurveyController> _logger;

        public OutcomeSurveyController(SurveyService surveyService, ProgramService programService, SurveyDataService surveyDataService,
            IMapper mapper, ILogger<OutcomeSurveyController> logger)
        {
            _surveyService = surveyService;
            _programService = programService;
            _surveyDataService = surveyDataService;
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

            return RedirectToAction("View", new { id = outcomeSurvey.Id });
        }

        public IActionResult ImportData(int id)
        {
            var outcomeSurvey = _surveyService.GetOutcomeSurvey(id);
            if (outcomeSurvey == null) return NotFound();

            _surveyDataService.DeleteData(outcomeSurvey);

            var program = _programService.GetProgram(outcomeSurvey.ProgramId);
            var responses = _surveyService.GetResponses(outcomeSurvey.SurveyId);
            var questions = _surveyService.GetQuestions(outcomeSurvey.SurveyId).ToDictionary(q => q.Id, q => q);
            var questionIdToOutcomeIndex = outcomeSurvey.QuestionIds.Select((id, index) => new { id, index })
                .ToDictionary(x => x.id, x => x.index);

            foreach (var response in responses)
            {
                foreach (var answer in response.Answers)
                {
                    if (questionIdToOutcomeIndex.ContainsKey(answer.QuestionId))
                    {
                        var surveyDataPoint = new SurveyDataPoint
                        {
                            ConstituencyType = outcomeSurvey.ConstituencyType,
                            Year = new Term((DateTime)(response.TimeSubmitted?.ToLocalTime())).Year,
                            ProgramId = outcomeSurvey.ProgramId,
                            OutcomeId = program.Outcomes[questionIdToOutcomeIndex[answer.QuestionId]].Id,
                            SurveyId = outcomeSurvey.SurveyId,
                            AnswerId = answer.Id
                        };

                        var question = questions[answer.QuestionId];
                        if (question.Type == QuestionType.Choice && question.MinSelection == 1 && question.MaxSelection == 1)
                        {
                            for (int i = 0; i < question.Choices.Count; ++i)
                                if (question.Choices[i] == answer.SingleSelection)
                                {
                                    surveyDataPoint.Value = i + 1;
                                    break;
                                }
                        }
                        else if (question.Type == QuestionType.Rating)
                        {
                            surveyDataPoint.Value = (int)answer.Rating;
                        }
                        else
                        {
                            _logger.LogWarning("Invalid question type {type} of question {question}", question.Type, question.Id);
                            continue;
                        }

                        _surveyDataService.AddData(surveyDataPoint);
                    }
                }
            }

            outcomeSurvey.DataImportTime = DateTime.UtcNow;
            _surveyDataService.SaveChanges();
            _logger.LogInformation("{user} imported data from outcome survey {survey}", User.Identity.Name, outcomeSurvey.Id);

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
