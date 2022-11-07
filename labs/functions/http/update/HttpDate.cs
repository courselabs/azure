using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CalendarProj
{
    public static class HttpDate
    {
        [FunctionName("HttpDate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ILogger log)
        {
            log.LogInformation("HTTP Date trigger activated");
            return new OkObjectResult(DateTime.UtcNow.ToShortDateString());
        }
    }
}
