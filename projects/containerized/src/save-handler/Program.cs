using ToDoList.Logging;
using ToDoList.Messaging;
using ToDoList.SaveHandler;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((builderContext, configBuilder) =>
                {
                    configBuilder.AddJsonFile("appsettings.json")
                                 .AddJsonFile("config/logging.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("config/config.json", optional: true, reloadOnChange: true)
                                 .AddJsonFile("secrets/secrets.json", optional: true, reloadOnChange: true)
                                 .AddEnvironmentVariables()
                                 .AddCommandLine(args);
                })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddConfiguredLogging(hostContext.Configuration)
                .AddMessaging()
                .AddToDoContext(hostContext.Configuration, ServiceLifetime.Scoped)
                .AddHostedService<NewItemSubscriber>();
    })
    .Build();

await host.RunAsync();
