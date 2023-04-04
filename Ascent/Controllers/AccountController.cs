using Ascent.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers;

[Authorize]
public class AccountController : Controller
{
    [AllowAnonymous]
    public ActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/" }, Constants.AuthenticationScheme.Oidc);
    }

    public IActionResult Logout()
    {
        return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, Constants.AuthenticationScheme.Oidc,
            Constants.AuthenticationScheme.CanvasCookie);
    }

    public IActionResult Profile()
    {
        return View();
    }
}
