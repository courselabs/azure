using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using PowerArgs;

namespace QueuePublisher
{
    class Program
    {
        static ServiceBusClient client;
        static ServiceBusSender sender;

        static async Task Main(string[] args)
        {
            var publisherId = Guid.NewGuid().ToString().Substring(0,4);
            var arguments = Args.Parse<PublisherArgs>(args);
            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(arguments.ConnectionString, clientOptions);
            sender = client.CreateSender(arguments.Queue);

            var batchCount=1;
            while (true)
            {
                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();
                for (int i = 1; i <= arguments.BatchSize; i++)
                {
                    messageBatch.TryAddMessage(new ServiceBusMessage($"Publisher: {publisherId}; batch: {batchCount}; message: {i}"));
                }
                await sender.SendMessagesAsync(messageBatch);
                batchCount++;

                Console.WriteLine($"Publisher: {publisherId}; sent batch number: {batchCount}; contained: {arguments.BatchSize} messages; sleeping for: {arguments.SleepSeconds}s");
                await Task.Delay(arguments.SleepSeconds*1000);
            }
        }
    }
}