using CampaignPerformanceAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CampaignPerformanceAnalytics.Controllers;

/// <summary>
/// Refuses an upload whose request is larger than <paramref name="maxRequestBytes"/> with the import page's
/// error message, before anything reads the body. Without it, the anti-forgery check (which reads the form
/// first) fails on the request size limit and the user sees a bare 400 page.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RejectOversizedImportAttribute(long maxRequestBytes, string message) : Attribute, IAuthorizationFilter, IOrderedFilter
{
    // Runs before [ValidateAntiForgeryToken], whose filter has order 1000.
    public int Order => -1000;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.ContentLength is not long length || length <= maxRequestBytes)
        {
            return;
        }

        var metadataProvider = context.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();
        context.Result = new ViewResult
        {
            ViewData = new ViewDataDictionary<ImportSummary>(metadataProvider, new ModelStateDictionary())
            {
                Model = ImportSummary.Failure("", message),
            },
        };
    }
}
