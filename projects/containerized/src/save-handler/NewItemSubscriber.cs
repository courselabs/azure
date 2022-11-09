using ToDoList.Messaging;
using StackExchange.Redis;
using ToDoList.Messaging.Messages.Events;
using ToDoList.Model;
using Serilog;

namespace ToDoList.SaveHandler;

public class NewItemSubscriber : BackgroundService
{
    private readonly ToDoContext _context;
    private readonly ConnectionMultiplexer _redis;

    private readonly RedisPublisher _publisher;

    private readonly IConfiguration _config;

    public NewItemSubscriber(ToDoContext context, RedisClient redisClient, RedisPublisher publisher, IConfiguration config)
    {
        _context = context;
        _redis = redisClient.ConnectionMultiplexer;
        _publisher = publisher;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();
        var listenChannel = NewItemEvent.MessageSubject;
        var subscription = await subscriber.SubscribeAsync(listenChannel);

        subscription.OnMessage(async channelMessage =>
        {
            Log.Debug($"Received message on channel: {channelMessage.Channel}");
            try
            {
                await HandleEvent(channelMessage.Channel, channelMessage.Message, stoppingToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to process message, channel: {channelMessage.Channel}");
            }
        });

        Log.Information($"Listening on channel: {listenChannel}");
    }

    private async Task HandleEvent(string eventType, string json, CancellationToken stoppingToken)
    {
        var message = MessageHelper.FromJson<NewItemEvent>(json);
        Log.Information($"Saving item, added: {message.Item.DateAdded}; event ID: {message.CorrelationId}");

        try
        {
            _context.ToDos.Add(message.Item);
            await _context.SaveChangesAsync();
            if (_config.GetValue<bool>($"Events:{ItemSavedEvent.MessageSubject}:Publish"))
            {
                await _publisher.Publish(new ItemSavedEvent(message.Item));
            }
            Log.Information($"Item saved; ID: {message.Item.ToDoId}; event ID: {message.CorrelationId}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Save FAILED; event ID: {message.CorrelationId}; exception: {ex}");
        }
    }
}