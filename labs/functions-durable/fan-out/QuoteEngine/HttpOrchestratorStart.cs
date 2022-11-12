using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace QuoteEngine
{
    public static class HttpOrchestratorStart
    {
        [FunctionName("HttpOrchestratorStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var input = new QuoteRequest
            {
                QuoteId = Guid.NewGuid(),
                ProductCode = "P101", // TODO - from req
                Quantity = 32 // TODO - from req
            };

            log.LogInformation($"Starting orchestration for quote: {input.QuoteId}; at: {DateTime.UtcNow} (UTC)");
            var instanceId = await starter.StartNewAsync("QuoteOrchestrator", input);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}