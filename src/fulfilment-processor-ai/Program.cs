using fulfilment_processor_ai;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.AddSingleton(new RoleNameInitializer("FulfilmentProcessor"));
    })
    .Build();

await host.RunAsync();
