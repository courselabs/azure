using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ToDoList.Messaging;
using ToDoList.Messaging.Messages.Events;
using ToDoList.Model;

namespace ToDoList.SaveHandler.Subscribers;

public class NewItemSubscriber : Subscriber<NewItemEvent>
{
    private readonly ToDoContext _context;
    private readonly MessageQueue _messageQueue;

    public NewItemSubscriber(ToDoContext context, MessageQueue messageQueue, ServiceBusClient client, IConfiguration config)
    : base(NewItemEvent.MessageSubject, client, config)
    {
        _context = context;
        _messageQueue = messageQueue;
    }

    protected override async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string json = args.Message.Body.ToString();
        var message = MessageHelper.FromJson<NewItemEvent>(json);
        Log.Information($"Saving item, added: {message.Item.DateAdded}; event ID: {message.CorrelationId}");

        try
        {
            _context.ToDos.Add(message.Item);
            await _context.SaveChangesAsync();
            if (_config.GetValue<bool>($"Events:{ItemSavedEvent.MessageSubject}:Publish"))
            {
                _messageQueue.Publish(new ItemSavedEvent(message.Item));
            }
            await args.CompleteMessageAsync(args.Message);
            Log.Information($"Item saved; ID: {message.Item.ToDoId}; event ID: {message.CorrelationId}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Save FAILED; event ID: {message.CorrelationId}; exception: {ex}");
        }
    }

    protected override Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}
