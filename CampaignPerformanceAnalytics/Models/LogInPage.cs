namespace CampaignPerformanceAnalytics.Models;

/// <summary>
/// What the login page shows: the username to keep filled in when logging in fails, and why it failed.
/// There is no password: the password box is always shown empty.
/// </summary>
public sealed record LogInPage(string? Username = null, string? ErrorMessage = null);
