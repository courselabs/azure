using ToDoList.Messaging.Messages;
using System.Text.Json;

namespace ToDoList.Messaging
{
    public class MessageHelper
    {
        public static string ToJson<TMessage>(TMessage message)
            where TMessage : Message
        {
            return JsonSerializer.Serialize(message);
        }

        public static TMessage FromJson<TMessage>(string json)
            where TMessage : Message
        {
            return JsonSerializer.Deserialize<TMessage>(json);
        }
    }
}
