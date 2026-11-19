using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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
