using Microsoft.Extensions.Configuration;
using Azure.Messaging.ServiceBus;
using ToDoList.Messaging.Messages;

namespace ToDoList.Messaging;

public class MessageQueue
{
    private readonly ServiceBusClient _client;
    public IConfiguration _config;

    public MessageQueue(ServiceBusClient client, IConfiguration config)
    {
        _client = client;
        _config = config;
    }

    public void Publish<TMessage>(TMessage message) where TMessage : Message
    {
        var sender = _client.CreateSender(message.Subject);
        var json = MessageHelper.ToJson(message);
        var sbMessage = new ServiceBusMessage(json);
        sender.SendMessageAsync(sbMessage);
    }
}