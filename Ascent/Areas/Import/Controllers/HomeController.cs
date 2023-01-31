using Ascent.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Import.Controllers
{
    [Area("Import")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
