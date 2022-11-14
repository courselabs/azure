using System;

namespace TopicToTableAndQueue
{
    public class QuoteResponseEntity
    {
        public Guid QuoteId 
        { 
            get { return new Guid(RowKey); } 
            set { RowKey = value.ToString(); }
        }
        public string SupplierCode
        { 
            get { return PartitionKey; } 
            set { PartitionKey = value; }
        }
        public double Quote {get; set;}

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}