using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    public class QuestionController : Controller
    {
        private readonly SurveyService _surveyService;

        private readonly IMapper _mapper;
        private readonly ILogger<QuestionController> _logger;

        public QuestionController(SurveyService surveyService, IMapper mapper, ILogger<QuestionController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(int surveyId)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            ViewBag.Survey = survey;

            return View(_surveyService.GetQuestions(surveyId));
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(int surveyId)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            ViewBag.Survey = survey;

            return View(new SurveyQuestionInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(int surveyId, SurveyQuestionInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var question = _mapper.Map<SurveyQuestion>(input);
            _surveyService.AddQuestionToSurvey(surveyId, question);
            _logger.LogInformation("{user} added question {question} to survey {survey}",
                User.GetName(), question.Id, surveyId);

            return RedirectToAction("Index", "Question", new { surveyId }, $"q-id-{question.Id}");
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var question = _surveyService.GetQuestion(id);
            if (question == null) return NotFound();

            ViewBag.Survey = question.Survey;

            return View(_mapper.Map<SurveyQuestionInputModel>(question));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, SurveyQuestionInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var question = _surveyService.GetQuestion(id);
            if (question == null) return NotFound();

            _mapper.Map(input, question);
            _surveyService.SaveChanges();
            _logger.LogInformation("{user} edited question {question}", User.GetName(), id);

            return RedirectToAction("Index", "Question", new { surveyId = question.SurveyId }, $"q-id-{question.Id}");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult MoveUp(int id)
        {
            var current = _surveyService.GetQuestion(id);
            if (current == null) return NotFound();

            if (current.Index > 0)
            {
                var previous = _surveyService.GetQuestion(current.SurveyId, current.Index - 1);
                current.Index--;
                previous.Index++;
                _surveyService.SaveChanges();
                _logger.LogInformation("{user} moved up question {question}", User.GetName(), id);
            }

            return RedirectToAction("Index", "Question", new { surveyId = current.SurveyId }, $"q-id-{current.Id}");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult MoveDown(int id)
        {
            var current = _surveyService.GetQuestion(id);
            if (current == null) return NotFound();

            if (current.Index < current.Survey.QuestionCount - 1)
            {
                var next = _surveyService.GetQuestion(current.SurveyId, current.Index + 1);
                current.Index++;
                next.Index--;
                _surveyService.SaveChanges();
                _logger.LogInformation("{user} moved down question {question}", User.GetName(), id);
            }

            return RedirectToAction("Index", "Question", new { surveyId = current.SurveyId }, $"q-id-{current.Id}");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public async Task<IActionResult> DeleteAsync(int id, int surveyId)
        {
            await _surveyService.DeleteQuestionAsync(id);
            _logger.LogInformation("{user} deleted question {question}", User.GetName(), id);
            return RedirectToAction("Index", new { surveyId });
        }
    }
}

namespace Ascent.Models
{
    public class SurveyQuestionInputModel
    {
        public QuestionType Type { get; set; } = QuestionType.Choice;

        public string Description { get; set; }

        public int SurveyId { get; set; }

        public int Index { get; set; }

        // Text Question

        [Display(Name = "Text Length")]
        public int TextLength { get; set; } = 20;

        // Rating Question

        [Display(Name = "Min Rating")]
        public int MinRating { get; set; } = 1;
        [Display(Name = "Max Rating")]
        public int MaxRating { get; set; } = 5;

        [Display(Name = "Include N/A Option")]
        public bool IncludeNotApplicable { get; set; } // Whether to include an N/A option

        // Choice Question

        public List<string> Choices { get; set; } = new List<string>() { "", "", "", "" };

        [Display(Name = "Min Selection")]
        public int MinSelection { get; set; } = 0;
        [Display(Name = "Max Selection")]
        public int MaxSelection { get; set; } = 4;
    }
}
