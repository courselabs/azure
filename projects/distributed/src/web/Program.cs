using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace ToDoList
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    configBuilder.AddJsonFile("appsettings.json")
                                 .AddJsonFile("config/logging.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("config/config.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("secrets/secrets.json", optional: true, reloadOnChange: true)
                                 .AddEnvironmentVariables()
                                 .AddCommandLine(args);

                    var config = configBuilder.Build();                    
                    if (config.GetValue<bool>("KeyVault:Enabled"))
                    {
                        var keyVaultName = config["KeyVault:Name"];
                        Console.WriteLine($"Adding KeyVault configuration source: {keyVaultName}");
                        configBuilder.AddAzureKeyVault(
                            new Uri($"https://{keyVaultName}.vault.azure.net/"),
                            new DefaultAzureCredential());
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
