using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using PowerArgs;

namespace Producer
{
    class Program
    {
        static EventHubProducerClient client;

        static async Task Main(string[] args)
        {
            var arguments = Args.Parse<ProducerArgs>(args);
            var producerClient = new EventHubProducerClient(arguments.ConnectionString, arguments.EventHub);

            for (int i = 0; i < arguments.ProducerCount; i++)
            {
                var producerId = Guid.NewGuid().ToString().Substring(0, 6);
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();
                for (int j = 1; j <= arguments.BatchSize; j++)
                {
                    var evt = new 
                    {
                        ProducerId = producerId,
                        EventId = Guid.NewGuid().ToString(),
                        EventNumber = j,
                        EventType = "Status",
                        Message = "OK"
                    };
                    var json = JsonSerializer.Serialize(evt);
                    eventBatch.TryAdd(new EventData(json));
                }

                await producerClient.SendAsync(eventBatch);
                Console.WriteLine($"Producer: {producerId}; sent: {arguments.BatchSize} events");
            }
            await producerClient.DisposeAsync();
        }
    }
}