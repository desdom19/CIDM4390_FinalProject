namespace CampaignPerformanceAnalytics.Models;

/// <summary>What kind of User someone is.</summary>
public enum Role
{
    Marketer,
    Admin,
}

/// <summary>
/// The one mapping between a Role and its text, "marketer" or "admin": the value in the users
/// table's role column, and the role kept in a logged-in User's cookie.
/// </summary>
public static class RoleValues
{
    public static string ToValue(this Role role) => role switch
    {
        Role.Marketer => "marketer",
        Role.Admin => "admin",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
    };

    public static Role Parse(string value) => value switch
    {
        "marketer" => Role.Marketer,
        "admin" => Role.Admin,
        _ => throw new InvalidOperationException($"Unknown role '{value}'."),
    };
}
