using Ascent.Models;
using Ascent.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public IActionResult Index(DateOnly? date)
        {
            var dates = _mftService.GetScoreDates();
            if (dates.Count == 0) return View(new List<MftScore>());

            ViewBag.SelectedDate = date ?? dates[0];
            ViewBag.Dates = dates.Select(d => new SelectListItem()
            {
                Text = d.ToString(),
                Value = d.ToString("O"),
                Selected = d == ViewBag.SelectedDate
            });

            return View(_mftService.GetScores(ViewBag.SelectedDate));
        }
    }
}
