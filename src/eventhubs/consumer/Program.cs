using System.Text.Json;
using Azure.Messaging.EventHubs.Consumer;
using Model;
using PowerArgs;

namespace Consumer;

class Program
{
    static async Task Main(string[] args)
    {
        var arguments = Args.Parse<ConsumerArgs>(args);
        var consumer = new EventHubConsumerClient(arguments.ConsumerGroup, arguments.ConnectionString, arguments.EventHub);
        var eventCount = 0;

        Console.WriteLine($"Reading up to: {arguments.ReadCount} events");
        await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync())
        {
            var json = partitionEvent.Data.EventBody.ToString();
            var evt = JsonSerializer.Deserialize<DeviceEvent>(json);
            Console.WriteLine($"Read producer: {evt.ProducerId} event number: {evt.EventNumber} from partition: {partitionEvent.Partition.PartitionId}; offset: {partitionEvent.Data.Offset}");
            if (++eventCount >= arguments.ReadCount)
            {
                break;
            }
        }        

        Console.WriteLine($"Read: {eventCount} events. Exiting.");
        await consumer.CloseAsync();
    }
}