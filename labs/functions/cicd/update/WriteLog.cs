using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ChainedFunctions
{
    public class UploadLog
    {
        [FunctionName("WriteLog")]  
        [StorageAccount("StorageConnectionString")]      
        public async Task Run(
            [BlobTrigger("heartbeat/{name}")] Stream uploadedBlob,
            [Table("heartbeats")] IAsyncCollector<HeartbeatLogEntity> entities,                          
            string name, ILogger log)
        {
            log.LogInformation($"New heartbeat blob uploaded:{name}");

            var entity = new HeartbeatLogEntity
            {
                PartitionKey = Guid.NewGuid().ToString().Substring(0,1),
                RowKey = Guid.NewGuid().ToString(),
                BlobName = name
            };
            await entities.AddAsync(entity);
            
            log.LogInformation("Recorded heartbeat in Table Storage");
        }
    }
}
