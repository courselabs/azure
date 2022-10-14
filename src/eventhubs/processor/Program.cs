using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Model;
using PowerArgs;
using System.Collections.Concurrent;

namespace Processor;

class Program
{
    static BlobContainerClient _StorageClient;
    static EventProcessorClient _Processor;
    static ConcurrentDictionary<string, int> _ProducerMessageCounts = new ConcurrentDictionary<string, int>();
    static ConcurrentDictionary<string, int> _PartitionMessageCounts = new ConcurrentDictionary<string, int>();

    static int _StatusUpdateFrequency;

    static async Task Main(string[] args)
    {
        var arguments = Args.Parse<ProcessorArgs>(args);
        _StatusUpdateFrequency = arguments.StatusUpdateFrequency;

        _StorageClient = new BlobContainerClient(arguments.StorageConnectionString, arguments.StorageContainer);
        _Processor = new EventProcessorClient(_StorageClient, arguments.ConsumerGroup, arguments.ConnectionString, arguments.EventHub);

        _Processor.ProcessEventAsync += ProcessEventHandler;
        _Processor.ProcessErrorAsync += ProcessErrorHandler;

        Console.WriteLine($"Processing events from consumer group: {arguments.ConsumerGroup}; owner ID: {_Processor.Identifier}");
        await _Processor.StartProcessingAsync();

        Console.ReadKey();
        await _Processor.StopProcessingAsync();
        Console.WriteLine("Stopped listening");
    }

    static async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        var evt = eventArgs.Data.EventBody.ToObjectFromJson<DeviceEvent>();
        _ProducerMessageCounts.AddOrUpdate(evt.ProducerId, 1, (id, count) => count+1);
        _PartitionMessageCounts.AddOrUpdate(eventArgs.Partition.PartitionId, 1, (id, count) => count+1);
        var messageCount = _PartitionMessageCounts.Sum(x=>x.Value);
        if (messageCount % _StatusUpdateFrequency == 0)
        {
            Console.WriteLine($"Read: {messageCount} messages; from {_ProducerMessageCounts.Keys.Count()} producers; partitions: {string.Join(',', _PartitionMessageCounts.Keys)}. Last message partition: {eventArgs.Partition.PartitionId}; offset: {eventArgs.Data.Offset}");        
        }
        await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
    }

    static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
    {
        Console.WriteLine($"Error processing message from partition: {eventArgs.PartitionId}; {eventArgs.Exception.Message}");
        return Task.CompletedTask;
    }
}
