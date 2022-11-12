using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace BlobToSql
{
    public class UploadLog
    {
        [FunctionName("UploadLog")]        
        [StorageAccount("UploadInputStorageConnectionString")]
        public async Task Run([BlobTrigger("uploads/{name}")] Stream uploadedBlob, 
                         string name, ILogger log,
                         [Sql("dbo.UploadLogItems", ConnectionStringSetting = "UploadSqlServerConnectionString")] IAsyncCollector<UploadLogItem> uploadLogs)
        {
            log.LogInformation($"New blob uploaded:{name}");

            var uploadLog = new UploadLogItem
            {
                Id = Guid.NewGuid(),
                BlobName = name,
                Size = uploadedBlob.Length
            };
            await uploadLogs.AddAsync(uploadLog);
            await uploadLogs.FlushAsync();
            
            log.LogInformation("Stored blob upload item in SQL Server");
        }
    }
}
