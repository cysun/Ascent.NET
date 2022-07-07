using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Mft.Controllers
{
    [Area("Mft")]
    public class ScoreController : Controller
    {
        private readonly MftService _mftService;

        public ScoreController(MftService mftService)
        {
            _mftService = mftService;
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
    }
}
