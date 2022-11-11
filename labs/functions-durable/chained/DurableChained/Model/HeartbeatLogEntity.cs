namespace DurableChained;

public class HeartbeatLogEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string BlobName { get; set; }
}
