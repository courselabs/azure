using System;

namespace BlobToSql
{
    public class UploadLogItem 
    {
        public Guid Id { get; set; }
        public string BlobName { get; set; }
        public long Size { get; set; }
    }
}