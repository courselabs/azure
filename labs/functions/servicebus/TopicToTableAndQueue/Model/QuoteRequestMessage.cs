using System;

namespace TopicToTableAndQueue
{
    public class QuoteRequestMessage
    {
        public Guid QuoteId {get; set;}
        public string ProductCode {get; set;}
        public int Quantity {get; set;}
    }
}