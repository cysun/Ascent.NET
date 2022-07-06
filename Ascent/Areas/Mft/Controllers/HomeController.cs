using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Mft.Controllers
{
    [Area("Mft")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
