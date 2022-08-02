using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    // Displays a page for non-OK results (e.g. NotFound()) returned by controller.
    public IActionResult Error(int id)
    {
        ViewBag.Code = id;
        switch (id)
        {
            case 404:
                return View("404");
            default:
                return View();
        }
    }

    // Logs unhandled exceptions and displays an error page.
    public IActionResult Exception()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        _logger.LogWarning("Exception {exception} caused by user {user}: \n{stackTrace}",
            feature?.Error?.Message, User.Identity.Name, feature?.Error?.StackTrace);

        return View();
    }
}
