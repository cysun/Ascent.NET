using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
