using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoList.Logging;
using ToDoList.Messaging;
using ToDoList.SaveHandler.Subscribers;

namespace ToDoList.SaveHandler;
class Program
{
    static async Task Main(string[] args)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("config/logging.json")
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
            config = configBuilder.Build();
        }

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IConfiguration>(config)
            .AddScoped<NewItemSubscriber>()
            .AddMessaging(config)
            .AddToDoContext(config, ServiceLifetime.Scoped)
            .AddConfiguredLogging(config)
            .BuildServiceProvider();

        var worker = serviceProvider.GetService<NewItemSubscriber>();
        await worker.Subscribe();
    }
}
