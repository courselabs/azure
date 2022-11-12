using ToDoList.Messaging.Messages;

namespace ToDoList.Messaging;

public class MessageQueue
{
    private readonly RedisPublisher _publisher;

    public MessageQueue(RedisPublisher publisher)
    {
        _publisher = publisher;
    }

    public void Publish<TMessage>(TMessage message) where TMessage : Message
    {
        _publisher.Publish(message).RunSynchronously();
    }
}