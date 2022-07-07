using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Mft.Controllers
{
    [Area("Mft")]
    public class IndicatorController : Controller
    {
        private readonly MftService _mftService;

        public IndicatorController(MftService mftService)
        {
            _mftService = mftService;
        }

        public IActionResult Index()
        {
            return View(_mftService.GetIndicators());
        }
    }
}
