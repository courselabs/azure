using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace QuoteEngine;

public class Supplier2Quote
{
    private const string SUPPLIER_CODE = "SUPPLIER-2";
    private static Random _Random = new Random();

    [FunctionName("Supplier2Quote")]
    public async Task<QuoteResponse> Run(
        [ActivityTrigger] QuoteRequest quoteRequest,
        ILogger log
    )
    {
        var sleep = _Random.Next(10000, 20000);
        log.LogInformation($"{SUPPLIER_CODE} waiting for: {sleep}ms");
        await Task.Delay(sleep);

        log.LogInformation($"{SUPPLIER_CODE} calculating price for quote ID: {quoteRequest.QuoteId}");
        var itemPrice = _Random.Next(5, 30);
        var response = QuoteBuilder.Build(quoteRequest, SUPPLIER_CODE, itemPrice);
        log.LogInformation($"{SUPPLIER_CODE} calculated quote: {response.Quote}; for ID: {quoteRequest.QuoteId}");

        return response;
    }
}