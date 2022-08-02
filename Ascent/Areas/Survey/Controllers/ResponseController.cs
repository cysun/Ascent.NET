using System.Text.Json;
using Ascent.Models;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Survey.Controllers
{
    [Area("Survey")]
    public class ResponseController : Controller
    {
        private readonly SurveyService _surveyService;

        private readonly ILogger<ResponseController> _logger;

        public ResponseController(SurveyService surveySerivce, ILogger<ResponseController> logger)
        {
            _surveyService = surveySerivce;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Edit(int surveyId)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            if (!survey.IsPublished)
                return RedirectToAction("Status", new { status = ResponseStatus.SurveyUnpublished });
            else if (survey.IsClosed)
                return RedirectToAction("Status", new { status = ResponseStatus.SurveyClosed });
            else if (!survey.AllowMultipleSubmissions)
            {
                var cookie = HttpContext.Request.Cookies["ascent-surveys-taken"];
                if (cookie != null && JsonSerializer.Deserialize<HashSet<int>>(cookie).Contains(surveyId))
                    return RedirectToAction("Status", new { status = ResponseStatus.SurveyTaken });
            }

            return View(new SurveyResponse(survey, _surveyService.GetQuestions(surveyId)));
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Edit([FromQuery] int surveyId, SurveyResponse response)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            if (!survey.IsPublished)
                return RedirectToAction("Status", new { status = ResponseStatus.SurveyUnpublished });
            else if (survey.IsClosed)
                return RedirectToAction("Status", new { status = ResponseStatus.SurveyClosed });

            var cookie = HttpContext.Request.Cookies["ascent-surveys-taken"];
            var surveysTaken = cookie == null ? new HashSet<int>() : JsonSerializer.Deserialize<HashSet<int>>(cookie);
            if (!survey.AllowMultipleSubmissions && surveysTaken.Contains(surveyId))
                return RedirectToAction("Status", new { status = ResponseStatus.SurveyTaken });

            _surveyService.AddResponseToSurvey(survey, response);
            _logger.LogInformation("survey {survey} received a response {response}", survey.Id, response.Id);

            surveysTaken.Add(surveyId);
            HttpContext.Response.Cookies.Append("ascent-surveys-taken", JsonSerializer.Serialize(surveysTaken),
                new CookieOptions() { Expires = DateTime.UtcNow.AddDays(21) });

            return RedirectToAction("Status", new { status = ResponseStatus.SurveyCompleted });
        }

        [AllowAnonymous]
        public IActionResult Status(ResponseStatus status)
        {
            return View(status);
        }
    }
}

namespace Ascent.Models
{
    public enum ResponseStatus
    {
        SurveyUnpublished, // the survey is not published yet
        SurveyClosed, // the survey is already closed
        SurveyTaken, // the user has already taken the survey and the survey does not allow multiple submissions
        SurveyCompleted // the survey is completed successfully
    }
}
