using System.Text.Json;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel;

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

        public List<SurveyResponse> Find(int questionId, int selection, string academicYear)
        {
            var responses = _surveyService.FindResponses(questionId, selection);

            var (startTime, endTime) = Utils.AcademicYear(academicYear);
            if (startTime != null)
                responses = responses.Where(r => startTime <= r.TimeSubmitted && r.TimeSubmitted <= endTime).ToList();

            return responses;
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

        public new IActionResult View(string id)
        {
            var response = _surveyService.GetResponse(id);
            if (response == null) return NotFound();

            return View(response);
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(string id)
        {
            var surveyId = _surveyService.DeleteResponse(id);
            if (surveyId == null) return NotFound();

            _logger.LogInformation("{user} deleted response {response}", User.GetName(), id);

            return RedirectToAction("Summary", new { surveyId });
        }

        public IActionResult Index(int surveyId)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            ViewBag.Survey = survey;
            var responseCounts = _surveyService.GetResponseTimes(surveyId)
                .GroupBy(t => new Term(t).AcademicYear)
                .ToDictionary(g => g.Key, g => g.Count());

            return View(responseCounts);
        }

        public IActionResult Summary(int surveyId, string academicYear)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            ViewBag.Questions = _surveyService.GetQuestions(surveyId);

            var (startTime, endTime) = Utils.AcademicYear(academicYear);
            var answersByQuestion = _surveyService.GetAnswers(surveyId, startTime, endTime)
                .GroupBy(a => a.QuestionId).ToDictionary(g => g.Key, g => g.ToList());
            if (answersByQuestion.Count == survey.QuestionCount)
                ViewBag.AnswersByQuestion = answersByQuestion;

            return View(survey);
        }

        public IActionResult Excel(int surveyId)
        {
            var survey = _surveyService.GetSurvey(surveyId);
            if (survey == null) return NotFound();

            var questions = _surveyService.GetQuestions(surveyId);
            var responses = _surveyService.GetResponses(surveyId);

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Responses");

            var row = sheet.CreateRow(0);
            row.CreateCell(0).SetCellValue("Timestamp");
            var questionIndex = 1;
            foreach (var question in questions)
                if (question.Type != QuestionType.Section)
                    row.CreateCell(questionIndex).SetCellValue("Q" + questionIndex++);

            var responseIndex = 1;
            foreach (var response in responses)
            {
                row = sheet.CreateRow(responseIndex++);
                row.CreateCell(0).SetCellValue(response.TimeSubmitted?.ToString("g"));
                var answerIndex = 1;
                for (int i = 0; i < response.Answers.Count; ++i)
                {
                    var answer = response.Answers[i];
                    answer.Question = questions[i];
                    if (answer.Question.Type != QuestionType.Section)
                    {
                        var answerText = Utils.StripHtmlTags(answer.GetAnswerAsText());
                        if (answerText != null && answerText.Length > 32767) // Excel cell limit
                            answerText = answerText.Substring(0, 32750) + "...[Truncated!]";
                        row.CreateCell(answerIndex++).SetCellValue(answerText);
                    }
                }
            }

            using var stream = new MemoryStream();
            workbook.Write(stream);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                survey.Name + " Responses.xlsx");
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
