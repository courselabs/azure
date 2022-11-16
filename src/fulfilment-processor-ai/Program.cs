using FulfilmentProcessor;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();        
        services.AddSingleton(new RoleNameInitializer("FulfilmentProcessor"));
        services.AddApplicationInsightsTelemetryWorkerService();
    })
    .Build();

await host.RunAsync();
