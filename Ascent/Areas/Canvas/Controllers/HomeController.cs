using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly CanvasApiService _canvasApiService;

        private readonly ILogger<HomeController> _logger;

        public HomeController(CanvasApiService canvasApiService, ILogger<HomeController> logger)
        {
            _canvasApiService = canvasApiService;
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = Constants.AuthenticationScheme.Canvas)]
        public async Task<IActionResult> IndexAsync()
        {
            return View(await _canvasApiService.GetCourses());
        }
    }
}
