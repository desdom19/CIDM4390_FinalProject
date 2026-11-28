using System.Security.Claims;
using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampaignPerformanceAnalytics.Controllers;

/// <summary>
/// The login page. Named AccountController to follow the ASP.NET convention for the login path;
/// everywhere else the app says "User" and "log in".
/// </summary>
public class AccountController : Controller
{
    private readonly UserService _userService;

    public AccountController(UserService userService)
    {
        _userService = userService;
    }

    private const string InvalidCredentialsMessage = "Invalid username or password.";
    private const string MissingInputMessage = "Enter a username and password.";
    private const string UnavailableMessage = "Logging in is unavailable right now. Try again later.";

    /// <summary>Shows the login page. A User who is already logged in goes straight to the Dashboard.</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(DashboardController.Index), "Dashboard");
        }

        return View(new LogInPage());
    }

    /// <summary>
    /// Logs the User in with a cookie holding their username and Role, then goes to the Dashboard.
    /// Any returnUrl is ignored: Success always goes to the Dashboard. On any other result the
    /// page is shown again with why, the username kept and the password box cleared.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LogInForm form)
    {
        var result = await _userService.LogInAsync(form.Username ?? "", form.Password ?? "");
        if (!result.Succeeded)
        {
            return View(new LogInPage(form.Username, MessageFor(result.Outcome)));
        }

        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, result.Username!),
                // Kept in the cookie, as "marketer" or "admin", for a later PBI to limit access by.
                new Claim(ClaimTypes.Role, result.Role!.Value.ToValue()),
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);
        // Not persistent: a session cookie, so closing the browser logs the User out (no remember-me).
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        return RedirectToAction(nameof(DashboardController.Index), "Dashboard");
    }

    /// <summary>
    /// Logs the User out and goes to the login page. POST-only with an anti-forgery token,
    /// so another site can't log a User out with a link or a forged form.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private static string MessageFor(LogInOutcome outcome) => outcome switch
    {
        LogInOutcome.InvalidCredentials => InvalidCredentialsMessage,
        LogInOutcome.MissingInput => MissingInputMessage,
        LogInOutcome.Unavailable => UnavailableMessage,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "No login page message for this outcome."),
    };
}
