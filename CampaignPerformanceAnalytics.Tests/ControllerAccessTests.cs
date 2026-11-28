using System.Reflection;
using CampaignPerformanceAnalytics.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Which pages need a logged-in User, read from the [Authorize] and [AllowAnonymous] attributes on the
/// controllers and their actions. No HTTP calls are made and no database is used; the tests stay
/// in the "Test database" collection only so that their checks are recorded in the login-logout report.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class ControllerAccessTests(VerificationReport report, ITestOutputHelper output)
{
    [Fact]
    public void Dashboard_and_Import_need_a_logged_in_User_and_the_login_page_does_not()
    {
        var checks = report.StartTest(ReportFeature.LogInLogOut, "01",
            "Dashboard and Import actions need a logged-in User; the Account login actions allow anyone", output);
        try
        {
            foreach (var controller in new[] { typeof(DashboardController), typeof(ImportController) })
            {
                foreach (var action in Actions(controller))
                {
                    checks.Equal($"{Describe(controller, action)} needs a logged-in User", true, NeedsLoggedInUser(controller, action));
                }
            }

            var loginActions = Actions(typeof(AccountController)).Where(a => a.Name == nameof(AccountController.Login)).ToList();
            // GET shows the form and POST submits it: both must exist and both must be open.
            checks.Equal("Account login actions (GET and POST)", 2, loginActions.Count);
            foreach (var action in loginActions)
            {
                checks.Equal($"{Describe(typeof(AccountController), action)} needs a logged-in User", false,
                    NeedsLoggedInUser(typeof(AccountController), action));
            }
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }

    /// <summary>The controller's public action methods, as MVC finds them.</summary>
    private static IEnumerable<MethodInfo> Actions(Type controller) =>
        controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName && m.GetCustomAttribute<NonActionAttribute>() is null);

    /// <summary>
    /// An action needs a logged-in User when it or its controller carries [Authorize] and neither
    /// carries [AllowAnonymous], which always wins.
    /// </summary>
    private static bool NeedsLoggedInUser(Type controller, MethodInfo action)
    {
        bool Has<T>() => controller.GetCustomAttributes(inherit: true).OfType<T>().Any()
                         || action.GetCustomAttributes(inherit: true).OfType<T>().Any();
        return Has<IAuthorizeData>() && !Has<IAllowAnonymous>();
    }

    private static string Describe(Type controller, MethodInfo action)
    {
        var verb = action.GetCustomAttribute<HttpPostAttribute>() is not null ? "POST" : "GET";
        return $"{controller.Name.Replace("Controller", "")}/{action.Name} {verb}";
    }
}
