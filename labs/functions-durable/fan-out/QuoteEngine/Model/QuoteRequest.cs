using System;

namespace QuoteEngine
{
    public class QuoteRequest
    {
        public Guid QuoteId {get; set;}
        public string ProductCode {get; set;}
        public int Quantity {get; set;}
    }
}