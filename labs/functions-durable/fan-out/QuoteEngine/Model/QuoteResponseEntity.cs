using System;

namespace QuoteEngine
{
    public class QuoteResponseEntity : QuoteResponse
    {
        public string PartitionKey 
        { 
            get { return SupplierCode; }
            set { SupplierCode = value; } 
        }

        public string RowKey 
        { 
            get { return QuoteId.ToString(); }
            set { QuoteId = new Guid(value); }
        }
    }
}