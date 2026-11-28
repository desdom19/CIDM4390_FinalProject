using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;

namespace CampaignPerformanceAnalytics.Controllers;

// On the whole controller, so the CSV import POST needs a logged-in User as well as the page.
[Authorize]
// no-store, so pressing Back after logging out can't show a saved copy of the page.
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ImportController : Controller
{
    /// <summary>The largest CSV file accepted: 5 MB.</summary>
    public const long MaxFileBytes = 5 * 1024 * 1024;

    // The request also carries the multipart boundaries and the anti-forgery token,
    // so its limit allows a little more than the file itself. A file just over 5 MB still
    // reaches the action; a larger request is refused before its body is read.
    private const long MaxRequestBytes = MaxFileBytes + 64 * 1024;

    private const string TooLargeMessage = "The file is larger than 5 MB. Only files up to 5 MB can be imported.";

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
    [RejectOversizedImport(MaxRequestBytes, TooLargeMessage)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Index(IFormFile? file)
    {
        // A browser sends a nameless, empty part when no file was chosen; it binds as null.
        if (file is null || string.IsNullOrEmpty(file.FileName))
        {
            return View(ImportSummary.Failure("", "No file was chosen. Choose a CSV file to import."));
        }

        var fileName = Path.GetFileName(file.FileName);

        if (!string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            return View(ImportSummary.Failure(fileName, "This is not a .csv file. Only .csv files can be imported."));
        }

        if (file.Length > MaxFileBytes)
        {
            return View(ImportSummary.Failure(fileName, TooLargeMessage));
        }

        await using var stream = file.OpenReadStream();
        var summary = await _importService.ImportAsync(stream, fileName);
        return View(summary);
    }
}
