using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ToDoList.Functions;

public class SaveItemHandler
{
    [FunctionName("SaveItemHandler")]
    public async Task Run(
        [ServiceBusTrigger("events.todo.newitem", Connection = "ServiceBusConnectionString")] NewItemEvent newItem,
        [Sql("dbo.ToDos", ConnectionStringSetting = "SqlServerConnectionString")] IAsyncCollector<ToDoItem> rows,
        [ServiceBus("events.todo.itemsaved", Connection = "ServiceBusConnectionString")] IAsyncCollector<ItemSavedEvent> messages,
        ILogger log)
    {
        log.LogInformation("New todo item from Service Bus");

        await rows.AddAsync(newItem.Item);
        await rows.FlushAsync();
        log.LogInformation("Saved Todo Item in SQL Server");

        var savedEvent = new ItemSavedEvent
        {
            Subject = "events.todo.itemsaved",
            SavedAt = DateTime.Now,
            Item = newItem.Item
        };
        await messages.AddAsync(savedEvent);
        log.LogInformation("Published Item Saved event to Service Bus");
    }
}
