using System;

namespace QuoteEngine
{
    public static class QuoteBuilder
    {        
        public static QuoteResponse Build(QuoteRequest quoteRequest, string supplierCode, double itemPrice)
        {
            return new QuoteResponse
            {
                SupplierCode = supplierCode,
                QuoteId = quoteRequest.QuoteId,
                Quote = itemPrice * quoteRequest.Quantity
            };
        }
   }
}
