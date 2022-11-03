using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ToDoList.Messaging;

public static class Dependencies
{

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration config)
    {        
        services.AddAzureClients(clientsBuilder =>
        {
            clientsBuilder.AddServiceBusClient(config["ConnectionStrings:ServiceBus"]);
        });
        services.AddSingleton<MessageQueue>();
        return services;
    }
}