using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    public class HomeController : Controller
    {
        private readonly SurveyService _surveyService;

        private readonly IMapper _mapper;
        private readonly ILogger<HomeController> _logger;

        public HomeController(SurveyService surveyService, IMapper mapper, ILogger<HomeController> logger)
        {
            _surveyService = surveyService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_surveyService.GetSurveys());
        }
    }
}
