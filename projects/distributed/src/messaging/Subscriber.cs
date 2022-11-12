using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Serilog;
using ToDoList.Messaging.Messages;

namespace ToDoList.Messaging;

public abstract class Subscriber<TMessage> where TMessage : Message
{
    protected abstract Task MessageHandler(ProcessMessageEventArgs args);

    protected abstract Task ErrorHandler(ProcessErrorEventArgs args);

    private readonly ServiceBusProcessor _processor;
    protected readonly IConfiguration _config;

    private ManualResetEvent _resetEvent = new ManualResetEvent(false);

    public Subscriber(string queue, ServiceBusClient client, IConfiguration config)
    {
        _processor = client.CreateProcessor(queue);
        _config = config;
    }

    public async Task Subscribe()
    {
        try
        {
            _processor.ProcessMessageAsync += this.MessageHandler;
            _processor.ProcessErrorAsync += this.ErrorHandler;
            
            await _processor.StartProcessingAsync();   
            Log.Information($"Subscribed to queue: {_processor.EntityPath}");         

            _resetEvent.WaitOne();           

            await _processor.StopProcessingAsync();
            Log.Information($"Stopped listening to queue: {_processor.EntityPath}");         
        }
        finally
        {
            await _processor.DisposeAsync();
        }
    }
}