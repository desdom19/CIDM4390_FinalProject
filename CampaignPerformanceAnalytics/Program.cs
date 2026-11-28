using CampaignPerformanceAnalytics.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Connection string comes from configuration (User Secrets in development).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
builder.Services.AddScoped(services =>
    new CsvImportService(connectionString, services.GetRequiredService<ILogger<CsvImportService>>()));
builder.Services.AddScoped(services =>
    new DashboardService(connectionString, services.GetRequiredService<ILogger<DashboardService>>()));
builder.Services.AddScoped(services =>
    new UserService(connectionString, services.GetRequiredService<ILogger<UserService>>()));

// A logged-in User is tracked with a cookie, checked against our own users table (ADR 0001: cookie authentication, not Identity).
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        // Logged out after 30 minutes without activity.
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        // Sliding expiration on its own only renews the cookie once half the 30 minutes has passed.
        // Renewing on every request makes each request restart the full 30 minutes.
        options.Events.OnValidatePrincipal = context =>
        {
            context.ShouldRenew = true;
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

await using (var connection = new MySqlConnection(connectionString))
{
    await connection.OpenAsync();
    await using var command = new MySqlCommand("SELECT CURRENT_USER();", connection);
    var user = await command.ExecuteScalarAsync();
    Console.WriteLine($"Connected successfully! Authenticated as: {user}");
}

// Adds the Marketer and the Admin if they are missing; never changes an existing User.
// Stops startup, naming the key, if a missing User's seed password isn't in User Secrets.
await new UserService(connectionString, app.Services.GetRequiredService<ILogger<UserService>>())
    .SeedAsync(app.Configuration[UserService.MarketerPasswordKey], app.Configuration[UserService.AdminPasswordKey]);



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
