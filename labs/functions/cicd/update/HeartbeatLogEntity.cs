using System;

namespace ChainedFunctions
{
    public class HeartbeatLogEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string BlobName {get; set;}
    }
}