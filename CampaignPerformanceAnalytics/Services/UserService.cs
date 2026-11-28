using CampaignPerformanceAnalytics.Models;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;

namespace CampaignPerformanceAnalytics.Services;

/// <summary>
/// The only code that reads or writes the users table: logs a User in, and seeds the Marketer and Admin.
/// </summary>
public class UserService
{
    // PasswordHasher ignores the user it is given, so the username stands in for it.
    private static readonly PasswordHasher<string> Hasher = new();

    private readonly string _connectionString;
    private readonly ILogger<UserService>? _logger;

    public UserService(string connectionString, ILogger<UserService>? logger = null)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<LogInResult> LogInAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return LogInResult.MissingInput();
        }

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(
                "SELECT username, password_hash, role FROM users WHERE username = @username;", connection);
            command.Parameters.AddWithValue("@username", username);
            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return LogInResult.InvalidCredentials();
            }

            var storedUsername = reader.GetString(0);
            var verification = Hasher.VerifyHashedPassword(storedUsername, reader.GetString(1), password);
            return verification == PasswordVerificationResult.Failed
                ? LogInResult.InvalidCredentials()
                : LogInResult.Success(storedUsername, RoleValues.Parse(reader.GetString(2)));
        }
        catch (MySqlException ex)
        {
            _logger?.LogError(ex, "Log in couldn't reach the database.");
            return LogInResult.Unavailable();
        }
    }

    /// <summary>The User Secrets key holding the Marketer's seed password.</summary>
    public const string MarketerPasswordKey = "Seed:MarketerPassword";

    /// <summary>The User Secrets key holding the Admin's seed password.</summary>
    public const string AdminPasswordKey = "Seed:AdminPassword";

    /// <summary>
    /// Adds the Marketer and the Admin if they don't exist, each with a hash of its seed password.
    /// An existing User is never changed, so its password is not needed. If a missing User has no
    /// password, nothing is added and the error names the User Secrets key to set.
    /// </summary>
    public async Task SeedAsync(string? marketerPassword, string? adminPassword)
    {
        var seedUsers = new[]
        {
            (Username: "marketer", Role: Role.Marketer, Password: marketerPassword, Key: MarketerPasswordKey),
            (Username: "admin", Role: Role.Admin, Password: adminPassword, Key: AdminPasswordKey),
        };

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        var existing = await ReadUsernamesAsync(connection);
        var missing = seedUsers.Where(u => !existing.Contains(u.Username)).ToList();

        var withoutPassword = missing.Where(u => string.IsNullOrWhiteSpace(u.Password)).ToList();
        if (withoutPassword.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", withoutPassword.Select(u =>
                $"The {u.Role} User '{u.Username}' doesn't exist yet and '{u.Key}' is not set. Set it with: "
                + $"dotnet user-secrets set \"{u.Key}\" \"<password>\" --project CampaignPerformanceAnalytics")));
        }

        foreach (var user in missing)
        {
            await AddUserAsync(connection, user.Username, user.Password!, user.Role);
            _logger?.LogInformation("Added the {Role} User '{Username}'.", user.Role, user.Username);
        }
    }

    private static async Task<HashSet<string>> ReadUsernamesAsync(MySqlConnection connection)
    {
        await using var command = new MySqlCommand("SELECT username FROM users;", connection);
        await using var reader = await command.ExecuteReaderAsync();
        // The column ignores case, so the check does too.
        var usernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync())
        {
            usernames.Add(reader.GetString(0));
        }
        return usernames;
    }

    private static async Task AddUserAsync(MySqlConnection connection, string username, string password, Role role)
    {
        await using var command = new MySqlCommand(
            "INSERT INTO users (username, password_hash, role) VALUES (@username, @hash, @role);", connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@hash", Hasher.HashPassword(username, password));
        command.Parameters.AddWithValue("@role", role.ToValue());
        await command.ExecuteNonQueryAsync();
    }
}
