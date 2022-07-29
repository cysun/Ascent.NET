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
    }
}

