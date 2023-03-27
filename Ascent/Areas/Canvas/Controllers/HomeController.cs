using Ascent.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize]
    public class HomeController : Controller
    {
        [Authorize(AuthenticationSchemes = Constants.AuthenticationScheme.Canvas)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
