using System;

namespace QuoteEngine
{
    public class QuoteResponse
    {
        public Guid QuoteId { get; set; }
        public string SupplierCode { get; set; }
        public double Quote {get; set;}
    }
}