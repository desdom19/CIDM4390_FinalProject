using CampaignPerformanceAnalytics.Services;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Connection string comes from configuration (User Secrets in development).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
builder.Services.AddScoped(_ => new CsvImportService(connectionString));

var app = builder.Build();

await using (var connection = new MySqlConnection(connectionString))
{
    await connection.OpenAsync();
    await using var command = new MySqlCommand("SELECT CURRENT_USER();", connection);
    var user = await command.ExecuteScalarAsync();
    Console.WriteLine($"Connected successfully! Authenticated as: {user}");
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
