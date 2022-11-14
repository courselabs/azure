using System;

namespace TopicToTableAndQueue
{
    public class QuoteStoredMessage
    {
        public Guid QuoteId {get; set;}
        public string SupplierCode {get; set;}
    }
}