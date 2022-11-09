using StackExchange.Redis;
using System.Threading.Tasks;
using ToDoList.Messaging.Messages;

namespace ToDoList.Messaging;

public class RedisPublisher
{
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger _logger;

    public RedisPublisher(RedisClient redisClient, ILogger<RedisPublisher> logger)
    {
        _redis = redisClient.ConnectionMultiplexer;
        _logger = logger;
    }

    public async Task Publish<TMessage>(TMessage message) where TMessage : Message
    {
        await PublishMessage(message, message.Subject);
    }

    private async Task PublishMessage<TMessage>(TMessage message, string channel)
        where TMessage : Message
    {
        _logger.LogDebug("Publishing message: {Type}, to channel: {Channel}", typeof(TMessage).Name, channel);
        var subscriber = _redis.GetSubscriber();
        var json = MessageHelper.ToJson(message);
        await subscriber.PublishAsync(channel, json);
        _logger.LogDebug("Publised message: {Type}, to channel: {Channel}", typeof(TMessage).Name, channel);        
    }
}