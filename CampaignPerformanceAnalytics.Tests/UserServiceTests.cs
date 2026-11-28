using CampaignPerformanceAnalytics.Models;
using CampaignPerformanceAnalytics.Services;
using Xunit.Abstractions;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// Integration tests through the UserService seam, against campaign_analytics_test.
/// Every test empties the users table, then adds its own Users through Seed with made-up
/// passwords. The real seed passwords in the app's User Secrets are never read.
/// Expected outcomes come from the login-logout spec and tickets, not from the code.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class UserServiceTests(TestDatabase database, VerificationReport report, ITestOutputHelper output)
    : IAsyncLifetime
{
    private const string MarketerPassword = "testpass-123";
    private const string AdminPassword = "adminpass-456";

    // xUnit creates a new instance per test, so this empties the users table before every test.
    public Task InitializeAsync() => database.EmptyUsersAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public Task Marketer_logs_in_with_the_right_password() =>
        RunAsync("01", "Marketer: right password gives Success with Role marketer", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);

            var result = await users.LogInAsync("marketer", MarketerPassword);

            checks.Equal("Outcome", LogInOutcome.Success, result.Outcome);
            checks.Equal("Username", "marketer", result.Username);
            checks.Equal("Role", (Role?)Role.Marketer, result.Role);
        });

    [Fact]
    public Task Admin_logs_in_with_the_right_password() =>
        RunAsync("01", "Admin: right password gives Success with Role admin", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);

            var result = await users.LogInAsync("admin", AdminPassword);

            checks.Equal("Outcome", LogInOutcome.Success, result.Outcome);
            checks.Equal("Username", "admin", result.Username);
            checks.Equal("Role", (Role?)Role.Admin, result.Role);
        });

    [Fact]
    public Task Username_capitalisation_is_ignored() =>
        RunAsync("01", "Username in capitals (ADMIN) still gives Success", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);

            var result = await users.LogInAsync("ADMIN", AdminPassword);

            checks.Equal("Outcome", LogInOutcome.Success, result.Outcome);
            checks.Equal("Role", (Role?)Role.Admin, result.Role);
        });

    [Fact]
    public Task Wrong_password_unknown_username_and_wrong_password_capitalisation_do_not_log_in() =>
        RunAsync("01", "Wrong password, unknown username, password in other capitals: no Success", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);

            checks.Equal("Wrong password succeeded", false, (await users.LogInAsync("marketer", "wrong-password")).Succeeded);
            checks.Equal("Unknown username succeeded", false, (await users.LogInAsync("nobody", MarketerPassword)).Succeeded);
            checks.Equal("Password in capitals succeeded", false, (await users.LogInAsync("marketer", "TESTPASS-123")).Succeeded);
            checks.Equal("Other User's password succeeded", false, (await users.LogInAsync("marketer", AdminPassword)).Succeeded);
        });

    [Fact]
    public Task Wrong_password_unknown_username_and_password_capitalisation_give_InvalidCredentials() =>
        RunAsync("02", "Wrong password, unknown username, password in other capitals: InvalidCredentials", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);

            checks.Equal("Wrong password", LogInOutcome.InvalidCredentials, (await users.LogInAsync("marketer", "wrong-password")).Outcome);
            checks.Equal("Unknown username", LogInOutcome.InvalidCredentials, (await users.LogInAsync("nobody", MarketerPassword)).Outcome);
            checks.Equal("Password in capitals", LogInOutcome.InvalidCredentials, (await users.LogInAsync("marketer", "TESTPASS-123")).Outcome);
        });

    [Fact]
    public Task Empty_or_whitespace_username_or_password_gives_MissingInput() =>
        RunAsync("02", "Empty or whitespace-only username or password: MissingInput", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);

            checks.Equal("Empty username", LogInOutcome.MissingInput, (await users.LogInAsync("", MarketerPassword)).Outcome);
            checks.Equal("Whitespace username", LogInOutcome.MissingInput, (await users.LogInAsync("   ", MarketerPassword)).Outcome);
            checks.Equal("Empty password", LogInOutcome.MissingInput, (await users.LogInAsync("marketer", "")).Outcome);
            checks.Equal("Whitespace password", LogInOutcome.MissingInput, (await users.LogInAsync("marketer", "  \t ")).Outcome);
            checks.Equal("Both empty", LogInOutcome.MissingInput, (await users.LogInAsync("", "")).Outcome);
        });

    [Fact]
    public Task Seed_adds_both_Users_to_an_empty_table() =>
        RunAsync("01", "Seed on an empty table adds the Marketer and the Admin", async checks =>
        {
            await database.CreateUserService().SeedAsync(MarketerPassword, AdminPassword);

            checks.SequenceEqual("Users and Roles",
                ["admin: admin", "marketer: marketer"],
                (await database.GetUsersAsync()).Select(u => $"{u.Username}: {u.Role}"));
        });

    [Fact]
    public Task Seed_run_again_never_overwrites_an_existing_User() =>
        RunAsync("01", "Seed run again with new passwords leaves both Users unchanged", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);
            var before = await database.GetUsersAsync();

            await users.SeedAsync("new-marketer-pass", "new-admin-pass");

            checks.SequenceEqual("Stored Users (hash unchanged)", before, await database.GetUsersAsync());
            checks.Equal("Marketer: original password", LogInOutcome.Success, (await users.LogInAsync("marketer", MarketerPassword)).Outcome);
            checks.Equal("Marketer: new password", LogInOutcome.InvalidCredentials, (await users.LogInAsync("marketer", "new-marketer-pass")).Outcome);
            checks.Equal("Admin: original password", LogInOutcome.Success, (await users.LogInAsync("admin", AdminPassword)).Outcome);
            checks.Equal("Admin: new password", LogInOutcome.InvalidCredentials, (await users.LogInAsync("admin", "new-admin-pass")).Outcome);
        });

    [Fact]
    public Task Seed_with_no_password_for_a_missing_User_names_that_User() =>
        RunAsync("01", "Seed with no password for a missing User: error names the User and its secret", async checks =>
        {
            var users = database.CreateUserService();

            var adminError = await SeedErrorAsync(users, MarketerPassword, null);
            checks.Contains("Admin missing: error", adminError, "admin", "Seed:AdminPassword");

            var marketerError = await SeedErrorAsync(users, "", AdminPassword);
            checks.Contains("Marketer missing: error", marketerError, "marketer", "Seed:MarketerPassword");
        });

    [Fact]
    public Task Seed_needs_no_password_for_a_User_that_exists() =>
        RunAsync("01", "Seed with no passwords once both Users exist: no error, Users unchanged", async checks =>
        {
            var users = database.CreateUserService();
            await users.SeedAsync(MarketerPassword, AdminPassword);
            var before = await database.GetUsersAsync();

            var error = await SeedErrorAsync(users, null, null);

            checks.Equal("Error", (string?)null, error);
            checks.SequenceEqual("Stored Users", before, await database.GetUsersAsync());
        });

    // ---- helpers ----

    /// <summary>Runs Seed and gives the message of the error it throws, or null if it throws none.</summary>
    private static async Task<string?> SeedErrorAsync(UserService users, string? marketerPassword, string? adminPassword)
    {
        try
        {
            await users.SeedAsync(marketerPassword, adminPassword);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Runs one test's body with its own set of recorded checks, then fails the test if any check
    /// didn't match. An exception is recorded in the report before it fails the test.
    /// </summary>
    /// <param name="sourceTicket">The login-logout ticket whose behaviour the test covers, e.g. "01".</param>
    private async Task RunAsync(string sourceTicket, string title, Func<TestChecks, Task> body)
    {
        var checks = report.StartTest(ReportFeature.LogInLogOut, sourceTicket, title, output);
        try
        {
            await body(checks);
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }
}

/// <summary>
/// Log in when the database can't be reached. No test database is needed: the connection string
/// points at a port nothing listens on. In the test database collection only to share the report.
/// </summary>
[Collection(TestDatabaseCollection.Name)]
public sealed class UserServiceDatabaseFailureTests(VerificationReport report, ITestOutputHelper output)
{
    private const string UnreachableConnectionString =
        "Server=127.0.0.1;Port=1;Database=campaign_analytics_test;User ID=nobody;Password=none;Connection Timeout=2";

    [Fact]
    public async Task Unreachable_database_gives_Unavailable_without_throwing()
    {
        var checks = report.StartTest(ReportFeature.LogInLogOut, "02",
            "Database can't be reached: Unavailable, nothing thrown", output);
        try
        {
            var result = await new UserService(UnreachableConnectionString).LogInAsync("marketer", "testpass-123");

            checks.Equal("Outcome", LogInOutcome.Unavailable, result.Outcome);
            checks.Equal("Username", (string?)null, result.Username);
            checks.Equal("Role", (Role?)null, result.Role);
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }

    [Fact]
    public async Task Empty_input_gives_MissingInput_without_checking_the_database()
    {
        var checks = report.StartTest(ReportFeature.LogInLogOut, "02",
            "Empty box with the database unreachable: MissingInput, so the database isn't checked", output);
        try
        {
            var users = new UserService(UnreachableConnectionString);

            checks.Equal("Empty username", LogInOutcome.MissingInput, (await users.LogInAsync("", "testpass-123")).Outcome);
            checks.Equal("Whitespace password", LogInOutcome.MissingInput, (await users.LogInAsync("marketer", "   ")).Outcome);
        }
        catch (Exception ex)
        {
            checks.Stopped(ex);
            throw;
        }
        checks.AssertAll();
    }
}
