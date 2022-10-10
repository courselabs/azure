using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using PowerArgs;

namespace Subscriber
{
    class Program
    {
        static bool AcknowledgeMessages;
        static ServiceBusClient client;
        static ServiceBusProcessor processor;

        static async Task Main(string[] args)
        {
            var arguments = Args.Parse<SubscriberArgs>(args);
            AcknowledgeMessages = arguments.AcknowledgeMessages;

            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(arguments.ConnectionString, clientOptions);
            if (string.IsNullOrEmpty(arguments.Topic) && string.IsNullOrEmpty(arguments.Subscription))
            {                
                processor = client.CreateProcessor(arguments.Queue, new ServiceBusProcessorOptions(){AutoCompleteMessages = false});
                Console.WriteLine($"Listening for messages on QUEUE: {arguments.Queue}");
            }
            else
            {
                processor = client.CreateProcessor(arguments.Topic, arguments.Subscription);
                Console.WriteLine($"Listening for messages on TOPIC: {arguments.Topic}; SUBSCRIPTION: {arguments.Subscription}");
            }

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;
                await processor.StartProcessingAsync();
                
                Console.ReadKey();

                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped listening");
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }
        
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received message: {body}");
            if (AcknowledgeMessages)
            {
                await args.CompleteMessageAsync(args.Message); 
                Console.WriteLine($"Acknowledged message: {body}");
            }
            
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}