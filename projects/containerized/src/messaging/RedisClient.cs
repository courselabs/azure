using StackExchange.Redis;

namespace ToDoList.Messaging;

public class RedisClient
{    
    public ConnectionMultiplexer ConnectionMultiplexer { get; private set; }

    public RedisClient(IConfiguration config, ILogger<RedisClient> logger)
    {
        ConnectionMultiplexer = ConnectionMultiplexer.Connect(config["ConnectionStrings:Redis"]);
        logger.LogInformation("Connected to Redis");
    }
}
