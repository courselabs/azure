using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace QuoteEngine;

public class Supplier3Quote
{
    private const string SUPPLIER_CODE = "SUPPLIER-3";
    private static Random _Random = new Random();

    [FunctionName("Supplier3Quote")]
    public async Task<QuoteResponse> Run(
        [ActivityTrigger] QuoteRequest quoteRequest,
        ILogger log
    )
    {
        log.LogInformation($"{SUPPLIER_CODE} calculating price for quote ID: {quoteRequest.QuoteId}");

        var itemPrice = _Random.Next(1, 14);
        var response = QuoteBuilder.Build(quoteRequest, SUPPLIER_CODE, itemPrice);
        log.LogInformation($"{SUPPLIER_CODE} calculated quote: {response.Quote}; for ID: {quoteRequest.QuoteId}");

        return response;
    }
}
