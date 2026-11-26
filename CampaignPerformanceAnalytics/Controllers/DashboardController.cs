using Microsoft.AspNetCore.Mvc;
using CampaignPerformanceAnalytics.Services;

namespace CampaignPerformanceAnalytics.Controllers;

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
