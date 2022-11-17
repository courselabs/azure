using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace ToDoList.Functions;

public static class Broadcast
{
    private static Random _Random = new Random();

    [FunctionName("broadcast")]
    public static async Task Run(
        [ServiceBusTrigger("events.todo.itemsaved", Connection = "ServiceBusConnectionString")] ItemSavedEvent savedEvent,
        [SignalR(HubName = "serverless", ConnectionStringSetting = "SignalRConnectionString")] IAsyncCollector<SignalRMessage> messages,
        ILogger log)
    {
        log.LogInformation("Item Saved event from Service Bus");

        var message = new SignalRMessage
        {
            Target = "newMessage",
            Arguments = new[] { savedEvent.Item }
        };
        await messages.AddAsync(message);
        log.LogInformation("Published broadcast to SignalR");
    }
}