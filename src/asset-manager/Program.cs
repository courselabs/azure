using AssetManager.Entities;
using AssetManager.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

var databaseName = builder.Configuration.GetValue<string>("Database:Name");
var connectionString = builder.Configuration.GetConnectionString(databaseName);

Console.WriteLine($"Using database name: {databaseName}; connection string: {connectionString}");

builder.Services.AddDbContext<AssetContext>(options => options.UseCosmos(connectionString, databaseName));
builder.Services.AddScoped<AssetService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
