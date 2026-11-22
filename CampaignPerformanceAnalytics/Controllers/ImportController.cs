using Microsoft.AspNetCore.Mvc;
using CampaignPerformanceAnalytics.Services;

namespace CampaignPerformanceAnalytics.Controllers;

public class ImportController : Controller
{
    private readonly CsvImportService _importService;

    public ImportController(CsvImportService importService)
    {
        _importService = importService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormFile? file)
    {
        if (file is null)
        {
            return View();
        }

        await using var stream = file.OpenReadStream();
        var summary = await _importService.ImportAsync(stream, file.FileName);
        return View(summary);
    }
}
