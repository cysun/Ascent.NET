using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    public class SurveyController : Controller
    {
        private readonly SurveyService _surveyService;

        private readonly IMapper _mapper;
        private readonly ILogger<SurveyController> _logger;

        public SurveyController(SurveyService surveyService, IMapper mapper, ILogger<SurveyController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_surveyService.GetSurveys());
        }

        public IActionResult View(int id)
        {
            var survey = _surveyService.GetSurvey(id);
            if (survey == null) return NotFound();

            ViewBag.Questions = _surveyService.GetQuestions(id);

            return View(survey);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new SurveyInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(SurveyInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var survey = _mapper.Map<Models.Survey>(input);
            _surveyService.AddSurvey(survey);
            _logger.LogInformation("{user} created survey {survey}", User.Identity.Name, survey.Id);

            return RedirectToAction("Index", "Question", new { surveyId = survey.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var survey = _surveyService.GetSurvey(id);
            if (survey == null) return NotFound();

            ViewBag.Survey = survey;
            return View(_mapper.Map<SurveyInputModel>(survey));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, SurveyInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var survey = _surveyService.GetSurvey(id);
            if (survey == null) return NotFound();

            _mapper.Map(input, survey);
            _surveyService.SaveChanges();
            _logger.LogInformation("{user} edited survey {survey}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var survey = _surveyService.GetSurvey(id);
            if (survey == null) return NotFound();

            if (!survey.IsDeleted)
            {
                survey.IsDeleted = true;
                _surveyService.SaveChanges();
                _logger.LogInformation("{user} deleted survey {survey}", User.Identity.Name, id);
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult Take(int id)
        {
            return RedirectToAction("Edit", "Response", new { surveyId = id });
        }
    }
}

namespace Ascent.Models
{
    public class SurveyInputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Publish Time")]
        public DateTime? TimePublished { get; set; }

        [Display(Name = "Close Time")]
        public DateTime? TimeClosed { get; set; }

        [Display(Name = "Allow multiple responses from one person")]
        public bool AllowMultipleSubmissions { get; set; }

        public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;
        public bool IsClosed => TimeClosed.HasValue && TimeClosed < DateTime.UtcNow;
    }
}

