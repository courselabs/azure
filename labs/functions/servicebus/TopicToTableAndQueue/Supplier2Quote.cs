using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TopicToTableAndQueue
{
    public class Supplier2Quote
    {
        private const string SUPPLIER_CODE="SUPPLIER-2";
        private static Random _Random = new Random();

        [FunctionName("Supplier2Quote")]
        public async Task Run(
            [ServiceBusTrigger("QuoteRequestTopic", "Supplier2Subscription", Connection = "ServiceBusInputConnectionString")] QuoteRequestMessage quoteRequest,
            [Table("quotes", Connection="OutputTableStorageConnectionString")] IAsyncCollector<QuoteResponseEntity> entities,
            [ServiceBus("QuoteStoredQueue", Connection = "ServiceBusOutputConnectionString")] IAsyncCollector<dynamic> messages,
            ILogger log
        )
        {
            var sleep = _Random.Next(1000,5000);
            log.LogInformation($"{SUPPLIER_CODE} waiting for: {sleep}ms");
            await Task.Delay(sleep);

            log.LogInformation($"{SUPPLIER_CODE} calculating price for quote ID: {quoteRequest.QuoteId}");
            
            var itemPrice = _Random.Next(5, 30);
            var entity = new QuoteResponseEntity
            {
                SupplierCode = SUPPLIER_CODE,
                QuoteId = quoteRequest.QuoteId,
                Quote = itemPrice * quoteRequest.Quantity
            };
            await entities.AddAsync(entity);
            log.LogInformation($"{SUPPLIER_CODE} saved quote response for ID: {quoteRequest.QuoteId}");

            var message = new QuoteStoredMessage 
            {                
                SupplierCode = SUPPLIER_CODE,
                QuoteId = quoteRequest.QuoteId
            };
            await messages.AddAsync(message);            
            log.LogInformation($"{SUPPLIER_CODE} published quote stored message for ID: {quoteRequest.QuoteId}");
        }
   }
}