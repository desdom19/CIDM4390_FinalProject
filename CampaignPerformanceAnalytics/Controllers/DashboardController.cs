using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampaignPerformanceAnalytics.Services;

namespace CampaignPerformanceAnalytics.Controllers;

[Authorize]
// no-store, so pressing Back after logging out can't show a saved copy of the page.
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _dashboardService.GetDashboardAsync());
    }
}
