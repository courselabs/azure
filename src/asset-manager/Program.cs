using AssetManager;
using Azure.Identity;
using System.IO;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("secrets/connectionstrings.json", optional: true, reloadOnChange: true);

if (builder.Configuration.GetValue<bool>("KeyVault:Enabled"))
{
    var keyVaultName = builder.Configuration["KeyVault:Name"];
    Console.WriteLine($"Adding KeyVault configuration source: {keyVaultName}");
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}

if (builder.Configuration.GetValue<bool>("LockFile:Enabled"))
{
    var lockFilePath = builder.Configuration["LockFile:Path"];
    Directory.CreateDirectory(lockFilePath);

    var hostName = Dns.GetHostName();
    var lockFileName = Path.Combine(lockFilePath, $"{hostName}.lock");
    Console.WriteLine($"Writing lockfile: {lockFileName}");

    var contents = $"Lock: {Guid.NewGuid()} at: {DateTime.UtcNow}{Environment.NewLine}";
    File.AppendAllText(lockFileName, contents);
}

builder.Services.AddRazorPages();

var dbApi = builder.Configuration.GetValue<string>("Database:Api");
var dbName = builder.Configuration.GetValue<string>("Database:Name");
Console.WriteLine($"Using database name: {dbName}; with API: {dbApi}");

var connectionString = builder.Configuration.GetConnectionString(dbName);
_ = dbApi switch
{
    "Sql" => Dependencies.AddSqlDatabaseServices(builder.Services, connectionString, dbName),

    "Mongo" => Dependencies.AddMongoDatabaseServices(builder.Services, connectionString, dbName),

    "BlobStorage" => Dependencies.AddBlobStorageServices(builder.Services, connectionString, dbName),

    _ => throw new NotSupportedException("Supported database APIs: Sql, Mongo & BlobStorage")
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
