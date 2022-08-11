using AssetManager;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

var dbApi = builder.Configuration.GetValue<string>("Database:Api");
var dbName = builder.Configuration.GetValue<string>("Database:Name");
Console.WriteLine($"Using database name: {dbName}; with API: {dbApi}");

var connectionString = builder.Configuration.GetConnectionString(dbName);
_ = dbApi switch
{
    "Sql" => Dependencies.AddSqlDatabaseServices(builder.Services, connectionString, dbName),

    "Mongo" => Dependencies.AddMongoDatabaseServices(builder.Services, connectionString, dbName),

    _ => throw new NotSupportedException("Supported database APIs: Sql and Mongo")
};

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
