using Microsoft.Extensions.DependencyInjection;

namespace ToDoList.Messaging;

public static class Dependencies
{

    public static IServiceCollection AddMessaging(this IServiceCollection services)
    { 
        services.AddSingleton<RedisClient>();
        services.AddSingleton<RedisPublisher>();
        services.AddSingleton<MessageQueue>();
        return services;
    }
}