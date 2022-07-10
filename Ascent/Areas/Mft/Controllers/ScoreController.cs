using Ascent.Models;
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
                return RedirectToAction("Index", new { year = year });

            var scores = _mftService.GetScores(year);
            if (scores.Count == 0)
                return RedirectToAction("Index", new { year = year });

            var distributionYear = distributionYears[0];
            foreach (var y in distributionYears)
                if (Math.Abs(y - year) < Math.Abs(distributionYear - year))
                    distributionYear = y;
            _logger.LogDebug("Selected distribution {distributionYear} for {year}", distributionYear, year);

            var distribution = _mftService.GetDistribution(distributionYear, "Student");
            if (distribution == null)
                return RedirectToAction("Index", new { year = year });

            foreach (var score in scores)
                score.Percentile = distribution.GetPercentile(score.Score);

            var stat = _mftService.GetScoreStat(year); // if scores exist, stat should exist.
            stat.MeanPercentile = distribution.GetPercentile(stat.Mean);
            stat.MedianPercentile = distribution.GetPercentile(stat.Median);

            distribution = _mftService.GetDistribution(distributionYear, "Institution");
            if (distribution != null)
                stat.InstitutionPercentile = distribution.GetPercentile(stat.Mean);

            _mftService.SaveChanges();
            _logger.LogInformation("{user} updated percentiles for {year} scores", User.Identity.Name, year);

            return RedirectToAction("Index", new { year = year });
        }
    }
}
