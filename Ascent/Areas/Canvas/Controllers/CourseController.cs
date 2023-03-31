using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize(AuthenticationSchemes = Constants.AuthenticationScheme.Canvas)]
    public class CourseController : Controller
    {
        private readonly CanvasApiService _canvasApiService;

        private readonly ILogger<CourseController> _logger;

        public CourseController(CanvasApiService canvasApiService, ILogger<CourseController> logger)
        {
            _canvasApiService = canvasApiService;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            return View(await _canvasApiService.GetCourses());
        }
    }
}
