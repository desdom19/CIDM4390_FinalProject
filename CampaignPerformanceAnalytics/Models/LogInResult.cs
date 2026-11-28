namespace CampaignPerformanceAnalytics.Models;

public enum LogInOutcome
{
    Success,

    /// <summary>A wrong password or an unknown username alike, so neither can be told apart.</summary>
    InvalidCredentials,

    /// <summary>The username or password was empty or only whitespace; the database was not checked.</summary>
    MissingInput,

    /// <summary>The database couldn't be reached. The error is logged, not thrown.</summary>
    Unavailable,
}

/// <summary>The result of a Log in. Username and Role are set only on Success.</summary>
public sealed record LogInResult(LogInOutcome Outcome, string? Username = null, Role? Role = null)
{
    public bool Succeeded => Outcome == LogInOutcome.Success;

    public static LogInResult Success(string username, Role role) => new(LogInOutcome.Success, username, role);

    public static LogInResult InvalidCredentials() => new(LogInOutcome.InvalidCredentials);

    public static LogInResult MissingInput() => new(LogInOutcome.MissingInput);

    public static LogInResult Unavailable() => new(LogInOutcome.Unavailable);
}
