using CampaignPerformanceAnalytics.Services;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace CampaignPerformanceAnalytics.Tests;

/// <summary>
/// The campaign_analytics_test database, shared by every test in the "Test database" collection.
/// Construction is the guard: it fails, and so every test fails, unless the connection string
/// names exactly campaign_analytics_test. The real campaign_analytics database is never used.
/// </summary>
public sealed class TestDatabase
{
    public const string RequiredDatabaseName = "campaign_analytics_test";
    private const string ConnectionStringName = "TestConnection";

    private readonly string _connectionString;

    public TestDatabase()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<TestDatabase>()
            .Build();

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not set in the test project's User Secrets. Set it with: "
                + $"dotnet user-secrets set \"ConnectionStrings:{ConnectionStringName}\" \"<connection string>\" "
                + "--project CampaignPerformanceAnalytics.Tests");

        var databaseName = new MySqlConnectionStringBuilder(connectionString).Database;
        if (!string.Equals(databaseName, RequiredDatabaseName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Refusing to run: the test connection string names database '{databaseName}', "
                + $"but the tests only run against '{RequiredDatabaseName}'.");
        }

        _connectionString = connectionString;
    }

    public CsvImportService CreateImportService() => new(_connectionString);

    public DashboardService CreateDashboardService() => new(_connectionString);

    public UserService CreateUserService() => new(_connectionString);

    /// <summary>Empties the users table. It has no link to the campaign tables.</summary>
    public Task EmptyUsersAsync() => ExecuteAsync("DELETE FROM users;");

    /// <summary>Empties results and campaigns. Channels are left alone.</summary>
    public async Task EmptyResultsAndCampaignsAsync()
    {
        // results first: it references campaigns.
        await ExecuteAsync("DELETE FROM results;");
        await ExecuteAsync("DELETE FROM campaigns;");
    }

    /// <summary>Every row of the users table, by username. The hash is compared, never shown in a report.</summary>
    public async Task<IReadOnlyList<StoredUser>> GetUsersAsync()
    {
        await using var connection = await OpenAsync();
        await using var command = new MySqlCommand(
            "SELECT username, role, password_hash FROM users ORDER BY username;", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var users = new List<StoredUser>();
        while (await reader.ReadAsync())
        {
            users.Add(new StoredUser(reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }
        return users;
    }

    public async Task<long> CountDailyResultsAsync() =>
        Convert.ToInt64(await ScalarAsync("SELECT COUNT(*) FROM results;"));

    public async Task<IReadOnlyList<StoredCampaign>> GetCampaignsAsync()
    {
        await using var connection = await OpenAsync();
        await using var command = new MySqlCommand(
            """
            SELECT c.campaign_name, ch.channel_name, c.start_date, c.end_date, c.roi_goal
              FROM campaigns c
              JOIN channels ch ON ch.channel_id = c.channel_id
             ORDER BY c.campaign_name;
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        var campaigns = new List<StoredCampaign>();
        while (await reader.ReadAsync())
        {
            campaigns.Add(new StoredCampaign(
                reader.GetString(0),
                reader.GetString(1),
                DateOnly.FromDateTime(reader.GetDateTime(2)),
                DateOnly.FromDateTime(reader.GetDateTime(3)),
                reader.IsDBNull(4) ? null : reader.GetDecimal(4)));
        }
        return campaigns;
    }

    public async Task<IReadOnlyList<StoredDailyResult>> GetDailyResultsAsync(string campaignName)
    {
        await using var connection = await OpenAsync();
        await using var command = new MySqlCommand(
            """
            SELECT r.result_date, r.spend, r.impressions, r.clicks, r.leads, r.revenue
              FROM results r
              JOIN campaigns c ON c.campaign_id = r.campaign_id
             WHERE c.campaign_name = @name
             ORDER BY r.result_date;
            """,
            connection);
        command.Parameters.AddWithValue("@name", campaignName);
        await using var reader = await command.ExecuteReaderAsync();
        var results = new List<StoredDailyResult>();
        while (await reader.ReadAsync())
        {
            results.Add(new StoredDailyResult(
                DateOnly.FromDateTime(reader.GetDateTime(0)),
                reader.GetDecimal(1),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetDecimal(5)));
        }
        return results;
    }

    private async Task<MySqlConnection> OpenAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using var connection = await OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<object?> ScalarAsync(string sql)
    {
        await using var connection = await OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        return await command.ExecuteScalarAsync();
    }
}

public record StoredCampaign(string Name, string ChannelName, DateOnly StartDate, DateOnly EndDate, decimal? RoiGoal);

public record StoredDailyResult(DateOnly Date, decimal Spend, int Impressions, int Clicks, int Leads, decimal Revenue);

/// <summary>A row of the users table. ToString leaves the hash out, so the report shows only its first characters.</summary>
public record StoredUser(string Username, string Role, string PasswordHash)
{
    public override string ToString() => $"{Username} ({Role}, hash {PasswordHash[..Math.Min(12, PasswordHash.Length)]}…)";
}

[CollectionDefinition(Name)]
public sealed class TestDatabaseCollection : ICollectionFixture<TestDatabase>, ICollectionFixture<VerificationReport>
{
    // Every test shares one database, so the collection also makes them run one at a time.
    public const string Name = "Test database";
}
