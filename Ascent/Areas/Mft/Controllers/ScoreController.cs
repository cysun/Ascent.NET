using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Mft.Controllers
{
    [Area("Mft")]
    public class ScoreController : Controller
    {
        private readonly MftService _mftService;

        private readonly ILogger<ScoreController> _logger;

        public ScoreController(MftService mftService, ILogger<ScoreController> logger)
        {
            _mftService = mftService;
            _logger = logger;
        }

        public IActionResult Index(int? year)
        {
            var stats = _mftService.GetScoreStats();

            if (stats.Count > 0)
            {
                ViewBag.Year = year ?? stats[0].Year;
                ViewBag.Scores = _mftService.GetScores(ViewBag.Year);
            }

            return View(stats);
        }

        public IActionResult Charts()
        {
            var stats = _mftService.GetScoreStats();
            if (stats.Count == 0) return RedirectToAction("Index");

            var years = stats.Select(i => i.Year).ToList();
            ViewBag.FromYear = years.Count >= 6 ? years[5] : years.Last();
            ViewBag.ToYear = years.First();
            ViewBag.Years = years;

            return View(stats);
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult UpdatePercentiles(int year)
        {
            var distributionYears = _mftService.GetDistributionYears();
            if (distributionYears.Count == 0)
                return RedirectToAction("Index", new { year });

            var scores = _mftService.GetScores(year);
            if (scores.Count == 0)
                return RedirectToAction("Index", new { year });

            var distributionYear = distributionYears[0];
            foreach (var y in distributionYears)
                if (Math.Abs(y - year) < Math.Abs(distributionYear - year))
                    distributionYear = y;
            _logger.LogDebug("Selected distribution {distributionYear} for {year}", distributionYear, year);

            var distribution = _mftService.GetDistribution(distributionYear, "Student");
            if (distribution == null)
                return RedirectToAction("Index", new { year });

            foreach (var score in scores)
                score.Percentile = distribution.GetPercentile(score.Score);

            var stat = _mftService.GetScoreStat(year); // if scores exist, stat should exist.
            stat.MeanPercentile = distribution.GetPercentile(stat.Mean);
            stat.MedianPercentile = distribution.GetPercentile(stat.Median);

            distribution = _mftService.GetDistribution(distributionYear, "Institution");
            if (distribution != null)
                stat.InstitutionPercentile = distribution.GetPercentile(stat.Mean);

            _mftService.SaveChanges();
            _logger.LogInformation("{user} updated percentiles for {year} scores", User.GetName(), year);

            return RedirectToAction("Index", new { year });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(int year, IFormFile uploadedFile)
        {
            var excelReader = new ExcelReader(uploadedFile.OpenReadStream());
            while (excelReader.Next())
                CreateOrUpdateScore(year, excelReader);
            _mftService.SaveChanges();

            CreateOrUpdateScoreStat(year);

            return RedirectToAction("Index", new { year });
        }

        private MftScore CreateOrUpdateScore(int year, ExcelReader excelReader)
        {
            var studentId = excelReader.Get("STUDENT ID");
            var score = _mftService.GetScore(year, studentId);
            if (score == null)
            {
                var (firstName, lastName) = Utils.SplitName(excelReader.Get("STUDENT NAME"));
                score = new MftScore
                {
                    Year = year,
                    StudentId = studentId,
                    FirstName = firstName,
                    LastName = lastName,
                    Score = int.Parse(excelReader.Get("TOTAL SCORE"))
                };
                _mftService.AddScore(score);
                _logger.LogInformation("New score [{cin}, {firstName} {lastName} {score}] added.",
                    studentId, firstName, lastName, score.Score);
            }

            return score;
        }

        private void CreateOrUpdateScoreStat(int year)
        {
            var scoreStat = _mftService.GetScoreStat(year);
            if (scoreStat == null)
            {
                scoreStat = new MftScoreStat { Year = year };
                _mftService.AddScoreStat(scoreStat);
            }

            var scores = _mftService.GetScores(year).Select(x => x.Score).ToList();
            if (scores.Count == 0) return;


            double median = 0;
            if (scores.Count % 2 == 0)
                median = (scores[scores.Count / 2] + scores[scores.Count / 2 - 1]) / 2.0;
            else
                median = scores[scores.Count / 2];

            scoreStat.Count = scores.Count;
            scoreStat.Mean = (int)Math.Ceiling(scores.Average());
            scoreStat.Median = (int)Math.Ceiling(median);

            _mftService.SaveChanges();
            _logger.LogInformation("{user} updated score stat for {year}", User.GetName(), year);
        }
    }
}
