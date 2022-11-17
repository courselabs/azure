using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace ToDoList.Functions;

public class SaveItemApi
{
    [FunctionName("items")]
    public async Task Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
        [ServiceBus("events.todo.newitem", Connection = "ServiceBusConnectionString")] IAsyncCollector<NewItemEvent> messages,
        ILogger log)
    {
        log.LogInformation("New todo item from HTTP");

        var item = await new StreamReader(req.Body).ReadToEndAsync();
        var todo = new ToDoItem
        {
            Item = item,
            DateAdded = DateTime.Now
        };

        var newItemEvent = new NewItemEvent
        {
            Subject = "events.todo.newitem",
            Item = todo
        };
        await messages.AddAsync(newItemEvent);

        log.LogInformation("Published New Item event to Service Bus");
    }
}