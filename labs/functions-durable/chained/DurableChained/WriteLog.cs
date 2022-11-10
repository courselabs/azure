using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableChained;

public class WriteLog
{
    [FunctionName("WriteLog")]
    public async Task Run(
        [ActivityTrigger] Tuple<AppStatus,string> statusWithBlobName,
        [Table("heartbeats", Connection = "StorageConnectionString")] IAsyncCollector<HeartbeatLogEntity> entities,
        ILogger log)
    {
        log.LogInformation($"WriteLog received status for: {statusWithBlobName.Item1.Component} with timestamp: {statusWithBlobName.Item1.TimestampUtc}");
    
        var entity = new HeartbeatLogEntity
        {
            PartitionKey = Guid.NewGuid().ToString().Substring(0, 1),
            RowKey = Guid.NewGuid().ToString(),
            BlobName = statusWithBlobName.Item2
        };
        await entities.AddAsync(entity);

        log.LogInformation("Recorded status in Table Storage");
    }
}
